using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;
using System.IO;
using UnityEngine.UI;
using TMPro;
using EZCameraShake;

public class PlayerMovement : MonoBehaviourPunCallbacks, IDamagable
{
    [Header("Refs")]
    public AmmoIndicator AmmoI;
    public Animator CameraAnimator;
    public TMP_Text Timerm;
    public TMP_Text Timers;
    public Transform ChatHolder;
    public GameObject TextPrefab;
    public GameObject NormalCrossHair;
    public CrossHairLerp CrossHair;
    public Slider S;
    public Transform DamageIndicator;
    public GameObject IndicatorPrefab;
    public GameObject DeathIndicatorPrefab;
    [SerializeField] Transform orientation;
    [SerializeField] GameObject Camera;
    [SerializeField] GameObject CameraHolder;
    [SerializeField] GameObject PlayerCanves;
    [SerializeField] GameObject UsernameCanves;
    [SerializeField] Animator TakeDamageScreen;
    [SerializeField] Health HealthBar;
    [SerializeField] public Transform Inventory;
    [SerializeField] InventoryUI InventoryUI;
    [SerializeField] public Animator GunHitAni;

    [Header("Movement")]
    [SerializeField] float moveSpeed = 6f;
    [SerializeField] float airMultiplier = 0.4f;
    float movementMultiplier = 10f;

    [Header("Sprinting")]
    [SerializeField] float walkSpeed = 4f;
    [SerializeField] float sprintSpeed = 6f;
    [SerializeField] float acceleration = 10f;

    [Header("Jumping")]
    public float jumpForce = 5f;

    [Header("Keybinds")]
    [SerializeField] KeyCode jumpKey = KeyCode.Space;
    [SerializeField] KeyCode sprintKey = KeyCode.LeftShift;

    [Header("Drag")]
    [SerializeField] float groundDrag = 6f;
    [SerializeField] float airDrag = 2f;

    float horizontalMovement;
    float verticalMovement;

    [Header("Ground Detection")]
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundMask;
    [SerializeField] float groundDistance = 0.2f;
    public bool isGrounded { get; private set; }
    private bool LastGrounded = false;
    private Vector3 LastGroundrbvel = Vector3.zero;

    [Header("ItemControl")]

    [SerializeField] public List<Item> items;

    [SerializeField] LayerMask ItemLayerMask;

    [HideInInspector] public float BobbingOffsetX;
    [HideInInspector] public float BobbingOffsetY;
    [Header("PublicGunSet")]
    [SerializeField] float GunAimMovementMutiplyer = 0.85f;

    float playerHeight = 2f;
    [HideInInspector] public int item_index;
    Vector3 moveDirection;
    Vector3 slopeMoveDirection;
    Rigidbody rb;
    RaycastHit slopeHit;
    const float MaxHealth = 100;
    float currentHealth = MaxHealth;
    [HideInInspector] public bool Aimming = false;
    [HideInInspector] public bool LockAimmingAndSwitching = false;
    float currentMoveMutiplyer = 1;
    float SprintMeter = 100;
    float currentTime = 0;
    bool IsApeared = true;
    [HideInInspector] public bool isMoving = false;
    [HideInInspector] public bool isSprinting = false;

    PhotonView PV;
    PhotonTransformViewClassic PT;
    public PlayerManeger PM {get; private set;}
    private List<int> checkI = new List<int>();
    private GunSwayAndBob GSAB;
    private float currentSpeed = 0 ;
    public Animator LeaderBoardAni;
    //Testing
    // private float LastTimeSwitched = 0; 
    // private int MoveVec = 1;

    //MainLoop
    private void Start()
    {
        checkI.Clear();
        if (PV.IsMine)
        {   
            EquipItem(0);
            InventoryUI.UpdateUI(items, 0);
            Destroy(UsernameCanves);
            Camera.AddComponent<CameraShaker>();
            GSAB = Inventory.GetComponent<GunSwayAndBob>();
            AmmoI.UpdateAmmoIndicotr();
            SwitchCrosshair();
            Destroy(GetComponent<MeshRenderer>());
        }
        else
        {
            Destroy(Camera);
            Destroy(rb);
            Destroy(PlayerCanves);
            GetComponent<MeshRenderer>().material =  SkinsManeger.Instance.skins[(int)PV.Owner.CustomProperties["Skin"]].mainskinmat;
        }
        

    }
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        Physics.gravity = new Vector3(0, -20 , 0);
        PV = GetComponent<PhotonView>();
        PM = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManeger>();
        PT = GetComponent<PhotonTransformViewClassic>();
    }

    private void Update()
    {   
        if (!PV.IsMine) return;
        Checkgrounded();
        MyInput();
        ControlDrag();
        ControlSpeed();
        JumpCheck();
        CalculateSlopeDir();
        LeaderBoardAni.SetBool("IsDown", Input.GetKey(KeyCode.E));
        if (items.Count <= 0) return;
        CrosshairAim();
        ItemSwitching();
        ItemUse();
        float VelOffsect = Mathf.Clamp(rb.velocity.y/20, -0.08f, 0.08f);
        GSAB.BobingOffset = new Vector2(BobbingOffsetX, BobbingOffsetY - VelOffsect);
        GSAB.SetBob((isMoving & isGrounded)? (isSprinting)? 1 : 0 :  2, (isMoving & isGrounded)? (isSprinting)? sprintSpeed : walkSpeed * 2 : 0);
        ItemThrow(); 
        
    }

    //UI

    public void SetTimer(int m, int s)
    {
        Timerm.SetText(m.ToString());
        if (s < 10)
        {
            Timers.SetText("0" + s.ToString());
        }
        else
        {
            Timers.SetText(s.ToString());
        }
    }
    public void SwitchCrosshair()
    {
        if (items.Count == 0)
        {
            NormalCrossHair.SetActive(true);
            CrossHair.gameObject.SetActive(false);
            return;
        }
        if (items[item_index].GetComponent<Gun>())
        {
            NormalCrossHair.SetActive(false);
            CrossHair.gameObject.SetActive(true);
        }
        else
        {
            NormalCrossHair.SetActive(true);
            CrossHair.gameObject.SetActive(false);
        }
    }

    public void DeactivateCrosshairs()
    {
        CrossHair.gameObject.SetActive(false);
        NormalCrossHair.gameObject.SetActive(false);
    }

    void DisplayChatText(string text, Color c)
    {
        if (text == "") return;
        if (text == null) return;
        TMP_Text T = Instantiate(TextPrefab, ChatHolder).GetComponent<TMP_Text>();
        T.SetText(text);
        T.color = c;
        Destroy(T.gameObject, 3);
    }

    private void FixedUpdate()
    {
        if (!PV.IsMine) return;
        MovePlayer();
        PT.SetSynchronizedValues(rb.velocity, 0);
        if (transform.position.y < -50)
        {
            Die(transform, "Void");
        }
    }


    //MovementFunctions
    void MyInput()
    {
        horizontalMovement = Input.GetAxisRaw("Horizontal");
        verticalMovement = Input.GetAxisRaw("Vertical");
        // if ((Time.time - LastTimeSwitched) > 3)
        // {
        //     MoveVec *= -1;
        //     LastTimeSwitched = Time.time;
        // }
        // horizontalMovement = MoveVec;
        if (horizontalMovement == 0 && verticalMovement == 0)
        {
            isMoving = false;
        }
        else
        {
            isMoving = true;
        }
        moveDirection = orientation.forward * verticalMovement + orientation.right * horizontalMovement;
    }
    void Jump()
    {
        if (isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
    }
    void ControlSpeed()
    {
        if (Input.GetKey(sprintKey) && isGrounded && isMoving && SprintMeter > 0f)
        {
            isSprinting = true;
            currentTime = -1;
            moveSpeed = Mathf.Lerp(moveSpeed, sprintSpeed, acceleration * Time.deltaTime);
            GetComponent<WallRun>().FovAdd = 10;
            SprintMeter -= Time.deltaTime  * 25;
            if (!IsApeared) 
            {
                S.GetComponent<Animator>().Play("Apear", -1, 0);
                IsApeared = true;
            }
            if (SprintMeter < 0) SprintMeter = 0;
        }
        else
        {
            isSprinting = false;
            if (currentTime == -1) currentTime = Time.time;
            if (Time.time - currentTime > 2 & SprintMeter != 0)
            {
                SprintMeter += Time.deltaTime * 15;
            }
            else if (Time.time - currentTime > 5)
            {
                SprintMeter += Time.deltaTime * 15;
            }
            if (SprintMeter > 100) SprintMeter = 100;
            moveSpeed = Mathf.Lerp(moveSpeed, walkSpeed, acceleration * Time.deltaTime);
            GetComponent<WallRun>().FovAdd = Aimming? ((GunInfo)items[item_index].itemInfo).GunAimFov : 0;
            if (IsApeared & SprintMeter == 100) 
            {
                S.GetComponent<Animator>().Play("Disapear", -1, 0);
                IsApeared = false;
            }
        }
        
        
        if (SprintMeter < 20)
        {
            S.transform.GetChild(0).GetComponent<Image>().color = Color.red;
        }
        else
        {
            S.transform.GetChild(0).GetComponent<Image>().color = Color.blue;
        }
        S.value = SprintMeter;
    }
    void ControlDrag()
    {
        if (isGrounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = airDrag;
        }
    }
    void MovePlayer()
    {
        currentSpeed = moveSpeed * ((items.Count > 0) ? (items[item_index].GetComponent<Gun>()) ? ((GunInfo)items[item_index].itemInfo).Weight : 1 : 1) * currentMoveMutiplyer;
        if (isGrounded && !OnSlope())
        {
            rb.AddForce(moveDirection.normalized * currentSpeed * movementMultiplier, ForceMode.Acceleration);
        }
        else if (isGrounded && OnSlope())
        {
            rb.AddForce(slopeMoveDirection.normalized * currentSpeed * movementMultiplier, ForceMode.Acceleration);
        }
        else if (!isGrounded)
        {
            rb.AddForce(moveDirection.normalized * currentSpeed * airMultiplier * movementMultiplier, ForceMode.Acceleration);
        }
    }
    void JumpCheck()
    {
        if (Input.GetKeyDown(jumpKey) && isGrounded)
        {
            Jump();
        }
    }
    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight / 2 + 0.5f))
        {
            if (slopeHit.normal != Vector3.up)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }
    void Checkgrounded()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded && !LastGrounded)
        {
            PT.SetSynchronizedValues(LastGroundrbvel, 0);
            CameraAnimator.SetTrigger("Land");
        }
        LastGroundrbvel = rb.velocity;
        LastGrounded = isGrounded;
    }
    void CalculateSlopeDir()
    {
        slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);
    }


    //ItemFunctions

    void CrosshairAim()
    {
        float m = 1;
        if (isMoving) m = 1.4f;
        if (isSprinting) m = 1.6f;
        if (items[item_index].GetComponent<Gun>()) CrossHair.LerpToScale(((GunInfo)items[item_index].itemInfo).HitOffset * 30 * m);
    }

    void ItemThrow()    
    {
        if (LockAimmingAndSwitching) return;
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (!items[item_index].GetComponent<Gun>()) return;
            PV.RPC("RPC_ThrowItem", RpcTarget.All);
        }
    }

    void ItemUse()
    {
        if (LockAimmingAndSwitching) return;
        if (Input.GetKey(KeyCode.Mouse0))
        {
            items[item_index].Use();
            AmmoI.UpdateAmmoIndicotr();
        }

        //GunAimmingControls

        if (Input.GetKeyDown(KeyCode.Mouse1)&& items[item_index].GetComponent<Gun>())
        {
            if (Aimming == false)
            {
                items[item_index].GetComponent<Gun>().Aim();
                Aimming = true;
            }
            else
            {
                items[item_index].GetComponent<Gun>().StopAim();
                Aimming = false;
            }
        }
        if (!isGrounded || Input.GetKey(sprintKey))
        {
            items[item_index].GetComponent<Gun>()?.StopAim();
            Aimming = false;
        }
        // if (Input.GetKeyDown(KeyCode.R))
        // {
        //     items[item_index].GetComponent<Gun>()?.Reload();
        // }
        currentMoveMutiplyer = Aimming ? GunAimMovementMutiplyer : 1;
    }

    void ItemSwitching()
    {
        if (LockAimmingAndSwitching) return;
        for (int i = 0; i < items.Count; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                EquipItem(i);
                Aimming = false;
                items[item_index].GetComponent<Gun>()?.StopAim();
                break;
            }
        }

        // if (Input.GetAxisRaw("Mouse ScrollWheel") > 0)
        // {
        //     if  (item_index >= items.Count - 1)
        //     {
        //         EquipItem(0);
        //         Aimming = false;
        //         items[item_index].GetComponent<Gun>()?.StopAim();
        //     }
        //     else
        //     {
        //         EquipItem(item_index + 1);
        //         Aimming = false;
        //         items[item_index].GetComponent<Gun>()?.StopAim();
        //     }
        // }
        // if (Input.GetAxisRaw("Mouse ScrollWheel") < 0)
        // {
        //     if  (item_index <= 0)
        //     {
        //         EquipItem(items.Count - 1);
        //         Aimming = false;
        //         items[item_index].GetComponent<Gun>()?.StopAim();
        //     }
        //     else
        //     {
        //         EquipItem(item_index - 1);
        //         Aimming = false;
        //         items[item_index].GetComponent<Gun>()?.StopAim();
        //     }
        // }
    }
    public void EquipItem(int _index)
    {
        if (items.Count == 0) return;
        for (int i = 0; i < items.Count; i++)
        {
            if (i != _index)
            {
                items[i].gameObject.SetActive(false);
            }
        }
        item_index = _index;
        items[item_index].gameObject.SetActive(true);

        if (PV.IsMine)
        {
            InventoryUI.UpdateUI(items, item_index);
            AmmoI.UpdateAmmoIndicotr();
            Hashtable hash = new Hashtable();
            hash.Add("itemIndex", item_index);
            hash.Add("Kills", PM.Kills);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
    }

    //Die

    void Die(Transform Player, string name)
    {
        PV.RPC("RPC_SpawnDeathObjects", RpcTarget.All, transform.position);
        PM.Die(Player, name);
        PM.SendPublicMesage(PV.Owner.NickName + "is killed by " + name);
    }


    //PhotonCallbacks   

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (!PV.IsMine && targetPlayer == PV.Owner)
        {
            if (changedProps["itemIndex"] != null) EquipItem((int)changedProps["itemIndex"]);
            // if (changedProps["Kills"] == null && targetPlayer == PV.Owner)
            // {
            //     Hashtable hash = new Hashtable();
            //     hash.Add("Kills", PM.Kills);
            //     PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
            // }
            return;
        }
        if (targetPlayer == PV.Owner)
        {
            if (changedProps["ClockUpdate"] != null) PM.SetClock((float)changedProps["ClockUpdate"]);
            if (changedProps["Mes"] != null) DisplayChatText((string)changedProps["Mes"], Color.white);
        }
    }

    public void TakeDamage(float _damage, int ID)
    {
        PV.RPC("RPC_TakeDamage", RpcTarget.All, _damage, ID);
    }

    public void TakeKnockBack(float _knockF, Vector3 dir)
    {
        PV.RPC("RPC_TakeKnockBack", RpcTarget.All, _knockF * dir.normalized);
    }


    public void IndicateDam(float amount)
    {
        GameObject e = Instantiate(IndicatorPrefab, DamageIndicator);
        e.GetComponent<TMP_Text>().SetText(amount.ToString());
        e.GetComponent<RectTransform>().localPosition += new Vector3(Random.Range(-25, 25), Random.Range(-25, 25), 0);
    }

    //PunRPC

    [PunRPC]

    void RPC_TakeDamage(float _damage, int ID)
    {
        if (!PV.IsMine) return;
        currentHealth -= _damage;
        HealthBar.UpdateValue(currentHealth);
        TakeDamageScreen.SetTrigger("TakeDam");
        if (currentHealth <= 0)
        {
            PhotonView pv = PhotonView.Find(ID);

            if (pv == null)
            {
                Debug.LogError("Can't find ID " + ID.ToString());
                return;
            }
            pv.GetComponent<PlayerMovement>().PM.AddKillCount();
            Transform T = pv.transform;
            string username = pv.Owner.NickName;
            pv.RPC("RPC_DisplayKill", RpcTarget.All, PV.Owner.NickName);
            Die(T, username);
        }
    }

    [PunRPC]
    void RPC_TakeKnockBack(Vector3 dirF)
    {
        if (!PV.IsMine) return;
        rb.AddForce(dirF, ForceMode.Impulse);
    }

    [PunRPC]
    void RPC_SpawnDeathObjects(Vector3 pos)
    {
        if(!PV.IsMine) return;
        for (int i = 0; i < items.Count; i++)
        {
            GameObject a = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "ItemsPickup", items[i].itemInfo.PickUpGameObject.name), pos + new Vector3(Random.Range(-1f, 1f), Random.Range(0f, 1f), Random.Range(-1f, 1f)), Quaternion.identity);
            PhotonNetwork.Destroy(items[i].itemPV);
        }
    }

    [PunRPC]
    void RPC_Get_Item(string ItemName)
    {
        if (PV.IsMine)
        {
            GameObject ItemPrefab = PM.FindItemPrefabInLibrary(ItemName);
            PhotonView item = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Items", ItemPrefab.name), Vector3.zero, Quaternion.identity).GetComponent<PhotonView>();
            PV.RPC("RPC_Get_Item_Snyc", RpcTarget.All, item.ViewID);
        }
        
    }

    [PunRPC]
    void RPC_Get_Item_Snyc(int PVID)
    {
        PhotonView Pv = PhotonView.Find(PVID);
        if (Pv == null)
        {
            Debug.LogError("Can't Find ID of" + PVID.ToString());
            return;
        }
        Pv.transform.SetParent(Inventory);
        Pv.transform.localPosition = Vector3.zero;
        Pv.transform.localEulerAngles = Vector3.zero;
        Pv.GetComponent<Item>().itemPV = Pv;
        Gun g = Pv.GetComponent<Gun>();
        if (g != null)
        {
            g.ammo = ((GunInfo)(g.itemInfo)).MagSize;
        }
        items.Add(Pv.GetComponent<Item>());
        item_index = items.Count - 1;
        EquipItem(item_index);
        if (PV.IsMine)
        {
            SwitchCrosshair();
        }
    }

    [PunRPC]

    void RPC_ThrowItem()
    {
        if (PV.IsMine)
        {
            int CurrentAmmo = items[item_index].GetComponent<Gun>().ammo;
            GunProjectile PU = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "GunsThrow", items[item_index].itemInfo.PickUpGameObject.name), transform.position + CameraHolder.transform.forward * 2f, Quaternion.identity).GetComponent<GunProjectile>();
            PU.Setup(CameraHolder.transform.forward * 30f, this);
            PhotonNetwork.Destroy(items[item_index].itemPV);
            BobbingOffsetX = 0;
            BobbingOffsetY = 0;
            Aimming = false;
            CameraShaker.Instance.ShakeOnce(10, 2, 0, 0.5f);
        }
        items.RemoveAt(item_index);
        EquipItem(0);
        item_index = 0;
        if (PV.IsMine)
        {
            InventoryUI.UpdateUI(items, 0);
            AmmoI.UpdateAmmoIndicotr();
            SwitchCrosshair();
        }
    }

    [PunRPC]

    void RPC_DisplayKill(string name)
    {
        if (!PV.IsMine) return;
        GameObject e = Instantiate(DeathIndicatorPrefab, DamageIndicator);
        e.GetComponent<TMP_Text>().SetText("KILL " + name);
        CameraShaker.Instance.ShakeOnce(5, 4, 0, 3);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        DisplayChatText(otherPlayer.NickName + " has left", Color.yellow);
    }
    
}