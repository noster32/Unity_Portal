using System.Collections.Generic;
using UnityEngine;

public class CTeleportObject : CComponent
{
    protected CPortal enterPortal;

    #region portalClone
    [Header("Portal Clone Object")]
    [SerializeField] protected GameObject grapicsObject;                    //메시 오브젝트

    [HideInInspector] public GameObject grapicsClone;                       //클론 오브젝트
    [HideInInspector] public Animator originalAnimator;                     //원본 애니메이터 
    [HideInInspector] public Animator cloneAnimator;                        //복제 애니메이터
    #endregion

    #region player
    [HideInInspector] public CPlayerMouseLook mouseLook;
    #endregion

    public Vector3 objectCenter = Vector3.zero;
    protected Collider objectCollider;
    protected Transform cameraTransform;
    private bool isPlayer;
    private bool isTurret;

    //터렛과 플레이어의 경우에는 그래픽 클론이 각각의 상속받은 스크립트에서 할당
    public override void Awake()
    {
        base.Awake();
        
        isPlayer = transform.CompareTag("Player");
        isTurret = transform.CompareTag("Turret");

        if (!isPlayer && !isTurret)
        {
            grapicsClone = new GameObject();
            grapicsClone.SetActive(false);
            var meshFilter = grapicsClone.AddComponent<MeshFilter>();
            var meshRenderer = grapicsClone.AddComponent<MeshRenderer>();

            meshFilter.mesh = grapicsObject.GetComponent<MeshFilter>().mesh;
            meshRenderer.materials = grapicsObject.GetComponent<MeshRenderer>().materials;
            grapicsClone.transform.parent = transform;
            grapicsClone.transform.localScale = new Vector3(1f, 1f, 1f);
        }
        
        if(!isTurret)
            objectCollider = GetComponent<Collider>();

        if (!m_oRigidBody)
            m_oRigidBody = grapicsObject.GetComponent<Rigidbody>();
    }

    public override void LateUpdate()
    {
        base.LateUpdate();
        
        if(!CSceneManager.Instance.portalPair.PlacedBothPortal())
            return;

        if(grapicsClone != null && grapicsClone.activeSelf)
        {
            grapicsClone.transform.position = enterPortal.GetOtherPortalRelativePoint(transform.position);
            grapicsClone.transform.rotation = enterPortal.GetOtherPortalRelativeRotation(transform.rotation);
        }
    }

    //건너편 포탈로 텔레포트
    public virtual void Teleport()
    {
        Vector3 temp = enterPortal.GetOtherPortalRelativePoint(transform.position + objectCenter);
        transform.position = temp - objectCenter;

        if(isPlayer && cameraTransform)
        {
            Quaternion resultRot = enterPortal.GetOtherPortalRelativeRotation(cameraTransform.rotation);
            transform.rotation = Quaternion.Euler(0f, resultRot.eulerAngles.y, 0f);
            mouseLook.TeleportCameraRotation(resultRot);
        }
        else
            transform.rotation = enterPortal.GetOtherPortalRelativeRotation(transform.rotation);

        if(m_oRigidBody)
            m_oRigidBody.velocity = enterPortal.GetOtherPortalRelativeDirection(m_oRigidBody.velocity);

        enterPortal = enterPortal.otherPortal;
    }

    //텔레포트 구역에 입장 시 클론 active
    public void EnterPortal(CPortal enterPortal)
    {
        this.enterPortal = enterPortal;

        if (grapicsClone)
            grapicsClone.SetActive(true);
    }

    //텔레포트 구역에서 퇴장 시 클론 disable
    public void ExitPortal()
    {
        if(grapicsClone)
            grapicsClone.SetActive(false);
    }

    //teleportObject와 벽간의 충돌 무시
    public void EnterPortalIgnoreCollision(List<Collider> wallCollider)
    {
        foreach (Collider c in wallCollider)
        {
            Physics.IgnoreCollision(objectCollider, c, true);
        }
    }

    //teleportObject와 벽간의 충돌 설정
    public void ExitPortalIgnoreCollision(List<Collider> wallCollider)
    {
        foreach (Collider c in wallCollider)
        {
            Physics.IgnoreCollision(objectCollider, c, false);
        }
    }

    //머터리얼 리스트 가져오기
    protected Material[] GetMaterials(GameObject target)
    {
        if (isPlayer)
        {
            SkinnedMeshRenderer sMRenderer = null;

            sMRenderer = target.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>();

            return sMRenderer.materials;
        }
        else
        {
            MeshRenderer[] mRenderer = null;

            mRenderer = target.GetComponentsInChildren<MeshRenderer>();

            var matList = new List<Material>();

            for(int renNum = 0; renNum < mRenderer.Length; ++renNum)
            {
                for(int matNum = 0; matNum < mRenderer[renNum].materials.Length; ++matNum)
                {
                    matList.Add(mRenderer[renNum].materials[matNum]);
                }
            }

            return matList.ToArray();
        }
    }
    
    //클론의 애니메이터에 원본의 애니메이터를 복사한다
    protected void GetAnimator(GameObject original, GameObject clone)
    {
        originalAnimator = original.GetComponent<Animator>();
        cloneAnimator = clone.GetComponent<Animator>();

        cloneAnimator.runtimeAnimatorController = originalAnimator.runtimeAnimatorController;
    }
}
