using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomersManager : MonoBehaviour
{

    public static CustomersManager instance;

    [SerializeField] List<CustomerController> _customerPrefabs = new List<CustomerController>();
    [SerializeField] List<CustomerController> _customersQueue = new List<CustomerController>();
    [SerializeField] List<Transform> _waitPoints = new List<Transform>();
    [SerializeField] Transform _spawnPoint;
    [SerializeField] float _spawnTime = 5f;
    int _maxCustomers, _currentQueue = 0;
    float _ctr = 0f;
    int lastCustomerIndex = -1;
    bool _canSpawn = true;

    private void Awake()
    {
        instance = this;
        _maxCustomers = _waitPoints.Count;
    }

    private void Update()
    {
        if(!GameManager.instance._isGameStarted) return;
        if (!_canSpawn)
        {
            _ctr = 0;
            return;
        }

        _ctr += Time.deltaTime;
        if (_ctr >= _spawnTime)
        {
            _ctr = 0f;
            SpawnCustomer();
        }

    }

    private void SpawnCustomer()
    {
        if (_customersQueue.Count >= _maxCustomers) return;

        var randIndex = UnityEngine.Random.Range(0, _customerPrefabs.Count);
        while (randIndex == lastCustomerIndex && lastCustomerIndex != -1)
        {
            randIndex = UnityEngine.Random.Range(0, _customerPrefabs.Count);
        }
        lastCustomerIndex = randIndex;
        CustomerController customer = Instantiate(_customerPrefabs[randIndex], _spawnPoint.position, Quaternion.identity);
        //CustomerController customer = Instantiate(_customerPrefabs[0], _spawnPoint.position, Quaternion.identity);
        customer.transform.SetParent(transform);
        _customersQueue.Add(customer);
        customer.SetTarget(_waitPoints[_currentQueue]);
        _currentQueue++;
    }

    public void RemoveCustomer()
    {
        _customersQueue.RemoveAt(0);
        _currentQueue--;
        for (int i = 0; i < _customersQueue.Count; i++)
        {
            _customersQueue[i].SetTarget(_waitPoints[i]);
        }
        _canSpawn = true;
    }

    public Transform GetSpawnPoint()
    {
        return _spawnPoint;
    }
}
