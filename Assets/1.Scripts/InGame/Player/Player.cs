using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;

public class Player : MonoBehaviour, IPicker
{
    public static Player Instance;
    public Transform Transform
    {
        get { return transform; }
    }
    public Rigidbody2D rg;
    public Joystick joystick;
    public Transform faceTr;
    public ParticleSystem pickParticle;
    public float angerAmount;
    public Color originColor = new Color(0.1921569f, 1f, 0.4705882f, 1f);
    public Color angerColor = new Color(1f, 0.1921569f, 0.1921569f, 1f); //FF3131 //분노
    public Color[] angerStepColors;
    public int angerStepIndex = 0;
    public AngerStep[] angerSteps;
    public SpriteRenderer bodySprite;
    public Transform bodyTr;

    public float moveSpeed;
    public float pickSpeed = 1f;
    public float pickTimer = 0f;
    Vector2 lastVec = Vector2.zero;
    public List<MagmaBall> magmaBalls = new List<MagmaBall>();
    public MagmaRotation magmaRotation;

    public Inventory inventory;
    public List<PlayerAbility> playerAbilities = new List<PlayerAbility>();


    public float pickRange = 2f;
    public LayerMask pickLayer;
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, pickRange);
    }

    private void Awake()
    {
        joystick = FindFirstObjectByType<Joystick>();
        Instance = this;
        rg = GetComponentInChildren<Rigidbody2D>();
        magmaBalls = GetComponentsInChildren<MagmaBall>().ToList();
        magmaRotation = GetComponentInChildren<MagmaRotation>();
        inventory = GetComponentInChildren<Inventory>();
    }
    void Start()
    {
        angerStepIndex = 0;
        StartCoroutine(CoBodyEffect());
    }
    void Update()
    {
        rg.linearVelocity = joystick.Direction * moveSpeed;

        if (joystick.Direction.magnitude > 0.1f)
        {
            if (joystick.Direction.x >= 0)
                faceTr.localScale = new Vector3(1, 1, 1);
            else
                faceTr.localScale = new Vector3(-1, 1, 1);

            lastVec = joystick.Direction;
        }

        pickTimer += Time.deltaTime;
        if (pickTimer >= pickSpeed)
        {
            pickTimer = pickSpeed;
            Pick();
        }

        angerAmount -= Time.deltaTime;
        if (angerAmount < 0)
            angerAmount = 0;

        magmaRotation.SetRotationSpeed(-(150 + angerAmount * 2));
        if (angerStepIndex < angerSteps.Length - 1)
        {
            bodySprite.color = Color.Lerp(angerSteps[angerStepIndex].color, angerSteps[angerStepIndex + 1].color, angerAmount / 100f);
        }
        else
        {
            bodySprite.color = angerSteps[angerStepIndex].color;
        }
        magmaBalls.ForEach(ball => ball.SetColor(bodySprite.color));
    }

    void UpdatePlayer()
    {

    }
    IEnumerator CoBodyEffect()
    {
        while (true)
        {
            yield return bodyTr.DOScale(angerSteps[angerStepIndex].breatheScale, angerSteps[angerStepIndex].breatheTime).SetEase(Ease.OutBack).WaitForCompletion();
            yield return bodyTr.DOScale(1f, angerSteps[angerStepIndex].breatheTime).SetEase(Ease.InBack).WaitForCompletion();
        }
    }

    void Pick()
    {
        //마지막 방향으로
        //lastVec 방향으로 쏘기
        Vector2 pickDir = lastVec.normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, pickDir, pickRange, pickLayer);
        if (hit.collider != null)
        {
            if (hit.collider.TryGetComponent(out IHittable hittable))
            {
                Pick(pickDir);
                return;
            }
        }
        else
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, pickRange, pickLayer);
            foreach (Collider2D collider in colliders)
            {
                if (collider.TryGetComponent(out IHittable hittable))
                {
                    Pick(hittable.Transform.position - transform.position);
                    return;
                }
            }
        }
    }

    void Pick(Vector2 pickDir)//IHittable hittable
    {
        pickParticle.transform.right = pickDir;
        pickParticle.Play();

        RaycastHit2D[] hit2Ds = Physics2D.RaycastAll(transform.position, pickDir, pickRange, pickLayer);
        for (int i = 0; i < hit2Ds.Length; i++)
        {
            if (hit2Ds[i].collider.TryGetComponent(out IHittable hittable))
            {
                hittable.OnHit(50);
            }
        }

        pickTimer = 0f;

        if (pickDir.x >= 0)
            faceTr.localScale = new Vector3(1, 1, 1);
        else
            faceTr.localScale = new Vector3(-1, 1, 1);

        angerAmount += 5f;
        if (angerAmount > 100f)
            angerAmount = 100f;

        if (angerStepIndex < angerSteps.Length - 1)
        {
            if (angerAmount > angerSteps[angerStepIndex].maxAngerAmount)
            {
                angerStepIndex++;
            }
        }

        if (angerStepIndex > 0)
        {
            if (angerAmount < angerSteps[angerStepIndex].minAngerAmount)
            {
                angerStepIndex--;
                if (angerStepIndex < 0)
                    angerStepIndex = 0;
            }
        }
    }

    public void PickUp(IPickable pickable)
    {
        pickable.PickedUp();
        inventory.AddItem(pickable.Key);
    }

    public void AddAbility(string key)
    {
        PlayerAbility playerAbility = playerAbilities.Where(a => a.key == key).FirstOrDefault();
        if (playerAbility == null)
        {
            playerAbility = new PlayerAbility { key = key };
            playerAbilities.Add(playerAbility);
        }

        playerAbility.count++;

        UpdatePlayer();
    }


}


[System.Serializable]
public class AngerStep
{
    public Color color;
    public float minAngerAmount;
    public float maxAngerAmount;
    public float breatheTime;
    public float breatheScale;
}

public class PlayerAbility
{
    public string key;
    public int count;
}