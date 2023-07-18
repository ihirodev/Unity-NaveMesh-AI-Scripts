using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIMinion : MonoBehaviour
{
    [SerializeField] GameObject _player;
    [SerializeField] float _runSpeed = 2.3f;
    [SerializeField] float _attackDistance = 1.3f;
    [SerializeField] Collider _enemyCollider;
    [SerializeField] float _crawlSpeed = 1.1f;
    [SerializeField] float _dragSpeed = 0.8f;
    private NavMeshAgent _nav;
    private Animator _anim;
    private float _distanceToPlayer;
    private bool _canMove = true;
    private float _navEnemySpeed;
    private NavMeshObstacle _navObstacle;
    [Tooltip("1 = Running, 2 = Crawl, 3 = Drag")]
    [SerializeField] int _enemyType = 1;
    private AnimatorStateInfo _enemyInfo;
    private AnimatorStateInfo _enemyInfo2;
    private AnimatorStateInfo _enemyInfo3;
    private bool _moving = true;
    private bool _isDead = false;
    [SerializeField] float _rotationSpeed = 2.0f;
    // Start is called before the first frame update
    void Start()
    {
        //Using NavMesh so we must initialize the appropiate components
        _nav = GetComponent<NavMeshAgent>();
        _player = GameObject.FindGameObjectWithTag("Player");
        _anim = GetComponent<Animator>();
        _navObstacle = GetComponent<NavMeshObstacle>();
        _navObstacle.enabled = false;
        if(_enemyType == 1)
        {
            _navEnemySpeed = _runSpeed;
        }
        if(_enemyType == 2)
        {
            _anim.SetLayerWeight(1, 1);
            _navEnemySpeed = _crawlSpeed;
        }
        if (_enemyType == 3)
        {
            _anim.SetLayerWeight(2, 1);
            _navEnemySpeed = _dragSpeed;
        }

    }

    // Update is called once per frame
    void Update()
    {

        if (SaveScript._playerDead == false)
        {
            if (_enemyType == 1)
            {
                _enemyInfo = _anim.GetCurrentAnimatorStateInfo(0);
            }
            else if (_enemyType == 2)
            {
                _enemyInfo2 = _anim.GetCurrentAnimatorStateInfo(1);
            }
            else if (_enemyType == 3)
            {
                _enemyInfo3 = _anim.GetCurrentAnimatorStateInfo(2);
            }

            if (_enemyInfo.IsTag("Dead") || _enemyInfo2.IsTag("Dead") || _enemyInfo3.IsTag("Dead"))
            {
                _moving = false;
                _nav.isStopped = true;
                //_canMove = false;
            }
            else
            {
                _moving = true;
            }
            
            if (_moving)
            {
                _distanceToPlayer = Vector3.Distance(_player.transform.position, transform.position);
                //Zombie is as close to the player as it can get so we can execute the attack animation
                if (_distanceToPlayer < _attackDistance)
                {
                    _anim.SetBool("Attack", true);
                    _nav.isStopped = true;
                    _canMove = false;
                    Vector3 pos = (_player.transform.position - transform.position).normalized;
                    Quaternion posRotation = Quaternion.LookRotation(new Vector3(pos.x, 0, pos.z));
                    transform.rotation = Quaternion.Slerp(transform.rotation, posRotation, Time.deltaTime * _rotationSpeed);
                }
                else if (_distanceToPlayer > _attackDistance + 1)
                {
                    _anim.SetBool("Attack", false);
                    _nav.isStopped = false;
                    _canMove = true;
                }
                //Moves the zombie towards the player's position
                if (_canMove)
                {
                    _nav.speed = _navEnemySpeed;
                    _nav.SetDestination(_player.transform.position);
                }
            }
        }
        
    }

    public void EnemyDeath()
    {
        if (!_isDead)
        {
            _anim.SetTrigger("Death");
            _nav.enabled = false;
            _isDead = true;
            SaveScript._score += 1000;
        }
        
        //
    }

    public void EnemyBurnedDeath()
    {
        if (!_isDead)
        {
            if (_enemyType == 1)
            {
                _anim.SetTrigger("Burned");
                _nav.enabled = false;
                _isDead = true;
                SaveScript._score += 1000;
            }
            else
            {
                _anim.SetTrigger("Death");
                _nav.enabled = false;
                _isDead = true;
                SaveScript._score += 1000;
            }
        }
    }

    private bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        for(int i = 0; i < 30; i++)
        {
            Vector3 randomPoint = center + Random.insideUnitSphere * range;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = Vector3.zero;
        return false;
    }

    public void DestroyOnDeath()
    {
        StartCoroutine(WaitForDestroy());
    }

    IEnumerator WaitForDestroy()
    {
        yield return new WaitForSeconds(1.5f);
        SaveScript._enemyCount -= 1;
        Destroy(gameObject);
    }
}
