using System;
using System.Collections.Generic;
using System.ComponentModel;
using DefaultNamespace;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace QueueSystem
{
    public class Queue : MonoBehaviour
    {
        [SerializeField][SerializeReference] private List<BaseQue> _ques;
        [Sirenix.OdinInspector.ReadOnly][ShowInInspector] private int _currentStep=0;

        private void OnEnable() => Zero();

        private void Zero()
        {
            _currentStep = 0;
            _ques.ForEach(x => x.OnInit(gameObject));
            _ques[_currentStep].OnStart();
        }

        private void Update()
        {
            _ques[_currentStep].OnUpdate(Time.deltaTime);
            if (_ques[_currentStep].IsEnd) NextStep();
        }

        private void NextStep()
        {
            _ques[_currentStep].OnFinish();
            _currentStep++;
            if (_currentStep >= _ques.Count) _currentStep = 0;
            _ques[_currentStep].OnStart();
        }

        public void CopyFrom(Queue otherQueue)
        {
            _ques = otherQueue._ques.DeepCloneJson();
            Zero();
        }
    }
}