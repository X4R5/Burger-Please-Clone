using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class CustomerController : MonoBehaviour
{
    NavMeshAgent _agent;
    Animator _animator;
    Transform _target;
    ChairController _currentChair;

    int _wantedBurgerCount;
    bool _waitingForTable = false, _startedEating = false;



    [SerializeField] int _minBurgerCount = 1, _maxBurgerCount = 3;
    int _currentBurgerCount = 0;
    [SerializeField] GameObject _burgerCanvas, _unavailableAnyTableCanvas;
    [SerializeField] TMP_Text _wantedBurgerCountText;
    [SerializeField] GameObject _tray;
    [SerializeField] List<GameObject> _allBurgers = new List<GameObject>();

    [SerializeField] float _eatingTime = 1.5f;
    float _eatingTimer = 0f;
    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _maxBurgerCount = _allBurgers.Count;
        _wantedBurgerCount = Random.Range(_minBurgerCount, _maxBurgerCount + 1);
        SetWantedBurgerCount();
    }

    private void Update()
    {
        if (_target == null) return;
        Move();
        Eat();
    }

    private void Eat()
    {
        if (_currentChair == null) return;
        if (_currentChair.GetPlate().activeSelf == false) return;
        if (!_startedEating) return;

        _eatingTimer += Time.deltaTime;
        if(_eatingTimer >= _eatingTime)
        {
            _currentChair.RemoveBurger();
            _eatingTimer = 0f;

            if (_currentChair.GetBurgerCount() == 0)
            {
                _currentChair = null;
                _startedEating = false;
                _agent.isStopped = false;
                _animator.SetBool("isRunning", true);
                _animator.SetBool("isSitting", false);
                SetTarget(CustomersManager.instance.GetSpawnPoint());

                StartCoroutine(AddMoney());
                
            }
        }
    }

    IEnumerator AddMoney()
    {
        for (int i = 0; i < _wantedBurgerCount; i++)
        {
            UIMoneyManager.instance.AddMoney(Camera.main.WorldToScreenPoint(transform.position), 10);
            yield return new WaitForSeconds(0.2f);
        }
    }

    private void Move()
    {

        //_agent.isStopped = true;
        //transform.position = _target.position;
        //var lookPos = _target.position;
        //lookPos.z = transform.position.z + 1f;
        //lookPos.y = transform.position.y;
        //transform.LookAt(lookPos);

        if (Vector3.Distance(transform.position, _target.position) <= 1f)
        {
            if (_target.CompareTag("Chair"))
            {
                SitChair();
            }
            else if (_target.CompareTag("WaitPoint"))
            {
                WaitInQueue();
            }
            else if (_target.CompareTag("SpawnPoint"))
            {
                Destroy(gameObject);
            }
        }
        else
        {
            _agent.isStopped = false;
            _agent.SetDestination(_target.position);
        }

        if (_waitingForTable)
        {
            var chair = ChairsManager.instance.GetRandomChair();
            if (chair != null)
            {
                HideUnavailableAnyTableCanvas();
                SetTarget(chair);
                chair.GetComponent<ChairController>().SetCustomer(this);
                _currentChair = chair.GetComponent<ChairController>();
                CustomersManager.instance.RemoveCustomer();
                _waitingForTable = false;
            }
            else
            {
                ShowUnavailableAnyTableCanvas();
            }
        }

        _animator.SetBool("isRunning", _agent.isStopped ? false : true);
    }

    private void WaitInQueue()
    {
        _agent.isStopped = true;
        transform.position = _target.position;
        var lookPos = _target.position;
        lookPos.z = transform.position.z + 1f;
        lookPos.y = transform.position.y;
        transform.LookAt(lookPos);
        _animator.SetBool("isRunning", false);
    }

    private void SitChair()
    {
        if (_animator.GetBool("isSitting")) return;

        _agent.isStopped = true;
        transform.position = _target.position;
        var lookPos = _target.position + _target.forward;
        transform.LookAt(lookPos);
        _animator.SetBool("isSitting", true);
        _animator.SetBool("isRunning", false);
        _tray.SetActive(false);

        _currentChair.SetCustomer(this);
        _currentChair.AddBurger(_wantedBurgerCount);
        _startedEating = true;
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    public void SetWantedBurgerCount()
    {
        _wantedBurgerCountText.text = _wantedBurgerCount.ToString();
    }

    public void ShowBurgerCanvas()
    {
        _burgerCanvas.SetActive(true);
    }

    public void HideBurgerCanvas()
    {
        _burgerCanvas.SetActive(false);
    }

    public void ShowUnavailableAnyTableCanvas()
    {
        _unavailableAnyTableCanvas.SetActive(true);
    }

    public void HideUnavailableAnyTableCanvas()
    {
        _unavailableAnyTableCanvas.SetActive(false);
    }

    public bool CanTakeBurger()
    {
        return _currentBurgerCount < _wantedBurgerCount;
    }

    public void TakeBurger()
    {
        if (_waitingForTable) return;

        if (_currentBurgerCount == 0)
        {
            _tray.SetActive(true);
            _animator.SetBool("isCarrying", true);
        }
        _allBurgers[_currentBurgerCount].SetActive(true);
        _currentBurgerCount++;

        if (_currentBurgerCount == _wantedBurgerCount)
        {
            _waitingForTable = true;
            HideBurgerCanvas();
        }
    }

    public void RemoveBurger()
    {
        _currentBurgerCount--;
        _allBurgers[_currentBurgerCount].SetActive(false);
        if (_currentBurgerCount == 0)
        {
            _tray.SetActive(false);
            _animator.SetBool("isCarrying", false);
        }
    }

    public void RemoveAllBurgers()
    {
        foreach (var burger in _allBurgers)
        {
            if (burger.activeSelf) burger.SetActive(false);
        }
        _currentBurgerCount = 0;
        _tray.SetActive(false);
        _animator.SetBool("isCarrying", false);
    }
}