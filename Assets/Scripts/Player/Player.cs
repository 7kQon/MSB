﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;

public class Player : MonoBehaviour {
    private GameManager gameManager;

    private Transform tr;
    private Rigidbody2D rb;
    private Animator animator;
    //Animation Parameter;
    public float curSpeed;
    public bool isGrounded;
    public bool isDoingBasicAtk;
    public bool isDoingSkill;

    public GameObject aimAxis;
    public GameObject basicAtkRange;
    private Sprite basicAtkRangeSprite;
    private Vector2 basicAtkRangePos;

    public GameObject skillRange;
    private Sprite skillRangeSprite;
    private Vector2 skillRangePos;

    private float aimAngle;

    public GameObject weaponAxis;
    private GameObject weapon;
    private Sprite weaponSprite;
    private Vector2 weaponPos;

    public int m_userIndex;
    public string m_userID;
    
    public int hp;
    private float moveSpeed;
    private float jumpForce;
    private float maxSpeed;

    public bool isMovable = true;
    private bool isAttacking = false;
    private bool isUsingSkill = false;

    public enum ACTION_TYPE
    {
        TYPE_ATTACK, TYPE_SKILL, TYPE_HIT
    }
    
    void Start () {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        tr = transform;
        rb = gameObject.GetComponent<Rigidbody2D>();
        animator = gameObject.GetComponent<Animator>();

        aimAxis = gameObject.transform.Find("AimAxis").gameObject;
        //basicAtkRange 는 weaponAxis의 자식으로 추가될 오브젝트의 basicAtkRange를 가져온다
        //skillRange 는 weaponAxis의 자식으로 추가될 오브젝트의 skillRange를 가져온다
        weaponAxis = gameObject.transform.Find("WeaponAxis").gameObject;
        //weapon = weaponAxis.transform.Find("TestWeapon").gameObject;

        hp = 5;
        moveSpeed = 50.0f;
        jumpForce = 300.0f;
        maxSpeed = 10.0f;

        isMovable = true;
        AttachWeapon(1);
        StartCoroutine("syncUserMove");
    }
	
	// Update is called once per frame
	void Update () {
        SetAnimatorParam(Mathf.Abs(rb.velocity.x), isGrounded);
    }

    private void FixedUpdate()
    {
    }

    public void SetAnimatorParam(float _speed,bool _isGrounded)
    {
        this.animator.SetFloat("Speed", _speed);
        this.animator.SetBool("Grounded", _isGrounded);
    }

    public void AttachWeapon(int index)    //플레이어 오브젝트에 달아줄 무기의 인덱스
    {
        
        //float posX = 0;
        //float posY = 0;
        switch (index)
        {
            case 1:
                //WeaponAxis의 자식으로 소드 프리팹을 붙혀준다
                weapon = Instantiate(Resources.Load<GameObject>("Prefabs/Weapon/SwordPrefab/Sword"), weaponAxis.transform) as GameObject;

                //소드 프리팹의 기본공격 범위와 스킬영역 범위
                basicAtkRange = Instantiate(Resources.Load<GameObject>("Prefabs/Weapon/SwordPrefab/BasicAtkRange"), aimAxis.transform) as GameObject;
                basicAtkRange.GetComponent<SpriteRenderer>().enabled = false;
                skillRange = Instantiate(Resources.Load<GameObject>("Prefabs/Weapon/SwordPrefab/SkillRange"), aimAxis.transform) as GameObject;
                skillRange.GetComponent<SpriteRenderer>().enabled = false;                
                break;
            case 2:
                //활 프리팹을 붙혀준다
                weapon = Instantiate(Resources.Load<GameObject>("Prefabs/Weapon/BowPrefab/Bow"), weaponAxis.transform) as GameObject;

                //활 프리팹의 기본공격 범위와 스킬영역 범위
                basicAtkRange = Instantiate(Resources.Load<GameObject>("Prefabs/Weapon/BowPrefab/BasicAtkRange"), aimAxis.transform) as GameObject;
                basicAtkRange.GetComponent<SpriteRenderer>().enabled = false;
                skillRange = Instantiate(Resources.Load<GameObject>("Prefabs/Weapon/BowPrefab/SkillRange"), aimAxis.transform) as GameObject;
                skillRange.GetComponent<SpriteRenderer>().enabled = false;
                break;
            default:
                break;
        }
    }

    public void Die() {
        // 리스폰 장소에서 다시 리스폰
    }

    public void Move(Vector3 moveVector) {
        if (!isMovable)
        {
            Debug.Log("return");
            return;
        }

        if (moveVector.x > 0f) 
        {
            Debug.Log("Joystick input right");
            if (rb.velocity.x > maxSpeed)
                return;

            rb.AddForce(Vector3.right * moveSpeed);    //AddForce는 Time.deltaTime을 곱해줄 필요가 없다         
            tr.localScale = new Vector3(0.5f,0.5f,0f);   //localScale을 좌우로 바꾼다
        }
        else if (moveVector.x < 0f)
        {
            Debug.Log("Joystick input left");
            if (rb.velocity.x < -maxSpeed)
                return;

            rb.AddForce(Vector3.left * moveSpeed);
            tr.localScale = new Vector3(-0.5f,0.5f,0f);
        }
        else
        {
            if (Input.GetKey(KeyCode.A))
            {
                //왼쪽으로 이동
                if (rb.velocity.x < -maxSpeed)
                    return;

                rb.AddForce(Vector3.left * moveSpeed);
                tr.localScale = new Vector3(-0.5f, 0.5f, 0f);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                //오른쪽으로 이동
                //Debug.Log("Joystick input right");
                if (rb.velocity.x > maxSpeed)
                    return;

                rb.AddForce(Vector3.right * moveSpeed);    //AddForce는 Time.deltaTime을 곱해줄 필요가 없다         
                tr.localScale = new Vector3(0.5f, 0.5f, 0f);   //localScale을 좌우로 바꾼다
            }
        }
    }

    public void Jump()
    {
        //Debug.Log("Jump!");
        //animator.SetBool("Grounded", false);
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded) 
        {
            rb.AddForce(Vector3.up * jumpForce);
        }
    }

    public float Vec32Angle(Vector3 vector)
    {
        float _aimAngle;
        if (vector == Vector3.zero)
            return 0;
        else
        {
            if (tr.localScale.x < 0)
                _aimAngle = Mathf.Rad2Deg * Mathf.Atan2(vector.x, vector.y) + 90;
            else
                _aimAngle = -Mathf.Rad2Deg * Mathf.Atan2(vector.x, vector.y) + 90;

            return _aimAngle;
        }
    }

    //기본 공격을 조준하면서 멀티터치로 동시에 스킬조준을 할 수 없다
    //이미 기본 공격을 하고 있거나, 스킬을 사용하고 있으면 조준선이 보이지 않는다
    public void Aim(Vector3 vector,string curRange)
    {
        if (vector == Vector3.zero)
        {
            aimAxis.transform.Find("BasicAtkRange").gameObject.GetComponent<SpriteRenderer>().enabled = false;
            aimAxis.transform.Find("SkillRange").gameObject.GetComponent<SpriteRenderer>().enabled = false;
            aimAxis.transform.localRotation = Quaternion.identity;
        }
        else
        {
            aimAngle = Vec32Angle(vector);
            aimAxis.transform.Find(curRange).gameObject.GetComponent<SpriteRenderer>().enabled = true;
            aimAxis.transform.localRotation = Quaternion.Euler(0, 0, aimAngle); //aimAxis를 회전 
        }

        aimAngle = Vec32Angle(vector);
        aimAxis.transform.Find(curRange).gameObject.GetComponent<SpriteRenderer>().enabled = true;
        aimAxis.transform.localRotation = Quaternion.Euler(0, 0, aimAngle); //aimAxis를 회전 
    }

    public void Attack(Vector3 vector)
    {
        // Hit Box 를 0.2 초간 enabled = true 후 다시 enable = false 시킨다
        // 추후 업그레이드
        // Animation을 실행시키고 Animation 프레임마다 BoxCollider로 HitBox 넣어줌 << 이게 제일 괜찮아 보임 판정상
        // +모든 공격은 조준을 해야되므로 플레이어가 조준한 곳으로 애니메이션, 히트박스가 생성되어야함
        // 이미 공격을 하고 있거나 스킬을 사용중에는 다시 Attack 이 호출되지 않는다
        if (!isAttacking)
        {
            isAttacking = true;
            aimAngle = Vec32Angle(vector);
            weaponAxis.transform.localRotation = Quaternion.Euler(0, 0, aimAngle);
            if (aimAngle >= -90 && aimAngle <= 90)
            {
                weaponAxis.transform.localScale = new Vector3(1, 1, 1);
                weaponAxis.transform.localRotation = Quaternion.Euler(0, 0, aimAngle);
            }
            else
            {
                weaponAxis.transform.localScale = new Vector3(-1, 1, 1);
                if (aimAngle > 0)
                    weaponAxis.transform.localRotation = Quaternion.Euler(0, 0, -(180 - aimAngle));
                else
                    weaponAxis.transform.localRotation = Quaternion.Euler(0, 0, -(180 + aimAngle));
            }
            weapon.GetComponent<Weapon>().isAttacking = true;

            StartCoroutine(WaitForIt());
            StartCoroutine(CoolTime());
            //weaponAxis.transform.localRotation = Quaternion.identity;
            //sendUserAttack();
        }
    }

    public void UseSkill(Vector3 vector)
    {
        isUsingSkill = true;
        aimAngle = Vec32Angle(vector);
        weaponAxis.transform.localRotation = Quaternion.Euler(0, 0, aimAngle);
        if (aimAngle >= -90 && aimAngle <= 90)
        {
            weaponAxis.transform.localScale = new Vector3(1, 1, 1);
            weaponAxis.transform.localRotation = Quaternion.Euler(0, 0, aimAngle);
        }
        else
        {
            weaponAxis.transform.localScale = new Vector3(-1, 1, 1);
            if (aimAngle > 0)
                weaponAxis.transform.localRotation = Quaternion.Euler(0, 0, -(180 - aimAngle));
            else
                weaponAxis.transform.localRotation = Quaternion.Euler(0, 0, -(180 + aimAngle));
        }
        weapon.GetComponent<Weapon>().isUsingSkill = true;

        StartCoroutine(WaitForIt());
        StartCoroutine(CoolTime());
    }

    private void OnTriggerEnter2D(Collider2D collision) //캐릭터가 땅에 닿아있으면 Grounded true;
    {
        //Debug.Log(collision.gameObject.tag);
        if (collision.gameObject.tag == "Ground")
        {
            isGrounded = true;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            isGrounded = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            isGrounded = false;
        }
    }

    IEnumerator syncUserMove()
    {
        while (true)
        {
            sendUserMove();
            yield return new WaitForSeconds(0.016f);
        }
    }

    public void sendUserMove()
    {
        JSONObject jsonData = new JSONObject();
        jsonData.AddField("positionX", this.gameObject.transform.position.x);
        jsonData.AddField("positionY", this.gameObject.transform.position.y);
        jsonData.AddField("positionZ", this.gameObject.transform.position.z);
        jsonData.AddField("toward", this.gameObject.transform.localScale.x);
        jsonData.AddField("forceX", this.gameObject.GetComponent<Rigidbody2D>().velocity.x);
        jsonData.AddField("forceY", this.gameObject.GetComponent<Rigidbody2D>().velocity.y);
        gameManager.sendUserMove(jsonData);
       // Debug.Log("userGameMove SENT");
    }

    public void sendUserAttack(bool isAttacking, float aimAngle)
    {
        JSONObject jsonData = new JSONObject();
        jsonData.AddField("action", ACTION_TYPE.TYPE_ATTACK.ToString());
        jsonData.AddField("aimAngle", aimAngle);    //애니메이션 실행되는 방향
        jsonData.AddField("animParam_isAttacking", isAttacking);    //애니메이션 실행여부
        gameManager.sendUserAction(jsonData);
        Debug.Log("userGameAction SENT");
    }

    public void sendUserSkill()
    {
        JSONObject jsonData = new JSONObject();
        jsonData.AddField("action", ACTION_TYPE.TYPE_SKILL.ToString());
        //jsonData.AddField("mel") 스킬 애니메이션에 필요한 것들을 가져와야댐
        gameManager.sendUserAction(jsonData);
        Debug.Log("userGameAction SENT");
    }

    public void sendUserHit(int targetUserIndex, Vector2 hitDirection, ACTION_TYPE actionType, float dmg)
    {
        JSONObject jsonData = new JSONObject();
        jsonData.AddField("target", targetUserIndex);
        jsonData.AddField("type", actionType.ToString());
        jsonData.AddField("hitDirectionX", hitDirection.x);
        jsonData.AddField("hitDirectionY", hitDirection.y);
        jsonData.AddField("damage", dmg);   //적용되는 데미지
        //jsonData.AddField("mel") 어떻게 누구를 때렸는지, CC기가 적용되는지 안되는지
        // 상대방이 어떻게 제어되는지까지 각 무기마다 다 다르기 때문에 Weapon을 가져오고
        gameManager.sendUserHit(jsonData);
        Debug.Log("userGameHit SENT");
    }

    IEnumerator WaitForIt()
    {
        yield return new WaitForSeconds(0.1f);
        //weaponAxis.transform.localRotation = Quaternion.identity;
    }

    IEnumerator CoolTime()
    {
        yield return new WaitForSeconds(0.5f);
        weaponAxis.transform.localScale = new Vector3(1, 1, 1);
        weaponAxis.transform.localRotation = Quaternion.identity;
        isAttacking = false;
    }
}
