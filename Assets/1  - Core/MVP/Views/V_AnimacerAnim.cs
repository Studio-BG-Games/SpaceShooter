using System;
using System.Collections.Generic;
using System.Linq;
using Animancer;
using CorePresenter;
using DIContainer;
using ManagerResourcess;
using ModelCore;
using ModelCore.Universal;
using ModelCore.Universal.AliasValue;
using Sirenix.Utilities;
using TMPro.SpriteAssetUtilities;
using UnityEngine;
using UnityEngine.PlayerLoop;
using ClipTransitionAsset = ManagerResourcess.ClipTransitionAsset;

namespace MVP.Views
{
    [AddComponentMenu(RootPresenter.PathToView+"V _ Anims")][RequireComponent(typeof(AnimancerComponent))]
    public class V_AnimacerAnim : ViewRootBase
    {
        private AnimancerComponent _animacer;
        
        [DI] private PackOfResources _resources;
        
        private List<RootModel> _clips;
        private List<Action> _unsubscribes = new List<Action>();
        private Dictionary<string, Action> _stopActions = new Dictionary<string, Action>();
        
        // Main
        private const string AliasContainer = "IdContainer";
        private const string AliasClip = "IdClip";
        private const string AliasIsPlaying = "IsPlaying";
        
        // Extra
        private const string AliasSpeed = "Speed";
        private const string AliasLayer = "Layer";
        private const string AliasEndEvent = "End";
        private const string AliasStopEvent = "Stop";
        
        protected override void CustomAwake() => _animacer = GetComponent<AnimancerComponent>();

        public override void View(RootModel engine) {
            _unsubscribes.ForEach(x=>x());
            _animacer.Stop();
            _unsubscribes.Clear();
            _stopActions.Clear();
            
            _clips = GetClips(engine);
            _clips.ForEach(x=>Subscribe(x));
        }

        private void Subscribe(RootModel rootModel) {
            var idClip = rootModel.Select<AliasString>(x => x.Alias == AliasClip);
            var idCont = rootModel.Select<AliasString>(x => x.Alias == AliasContainer);
            var IsPlayed = rootModel.Select<AliasBool>(x => x.Alias == AliasIsPlaying);

            var container = _resources.Get(idCont.Value) as ClipTransitionAsset;
            if (container == null) {
                Debug.LogWarning($"Нет контейнерf - {idCont.Value}");
                return;
            }
            var clip = container.Get(idClip.Value);
            if (clip == null) {
                Debug.LogWarning($"Нет клипа в контейнере - {idCont.Value} под id - {idClip.Value}");
                return;
            }

            IsPlayed.Update += PlayHandler(clip, rootModel);
            PlayHandler(clip, rootModel).Invoke(!IsPlayed.Value, IsPlayed.Value);
            _unsubscribes.Add(new Action(() => IsPlayed.Update -= PlayHandler(clip, rootModel)));
        }

        private Action<bool, bool> PlayHandler(ClipTransition clip, RootModel rootModel) {
            return (bool pastV, bool newV) => {
                if (pastV == newV) return;
                if (newV == false) {
                    if (_stopActions.TryGetValue(rootModel.Alias, out var r)) r();
                }
                else {
                    var layer = rootModel.GetIdT<AliasInt>($"I_{AliasLayer}");
                    var speed = rootModel.GetIdT<AliasFloat>($"F_{AliasSpeed}");
                    
                    var endEvent = rootModel.GetIdT<JsEvent>($"Event_{AliasEndEvent}");
                    var stopEvent = rootModel.GetIdT<JsEvent>($"Event_{AliasStopEvent}");
                    var otherEvent = rootModel.SelectAll<JsEvent>(x => true).Except(new []{endEvent, stopEvent});
                    
                    var state = _animacer.Layers[layer != null ? layer.Value : 0].Play(clip);
                    if (speed != null) state.Speed = speed.Value;
                    if (endEvent != null) state.Events.OnEnd = EndHandler(state, endEvent);
                    otherEvent.ForEach(x => { if (state.Events.Names.Contains(x.Alias)) state.Events.SetCallback(x.Alias, x.Trig); });
                    
                    if(!_stopActions.ContainsKey(rootModel.Alias)) {
                        _stopActions.Add(rootModel.Alias, new Action(() => {
                            stopEvent?.Trig();
                            otherEvent.ForEach(x => { if (state.Events.Names.Contains(x.Alias)) state.Events.RemoveCallback(x.Alias, x.Trig); });
                            state.Stop(); 
                        }));
                    }
                }
            };
        }

        private Action EndHandler(AnimancerState state, JsEvent endEvent) {
            return ()=> {
                endEvent.Trig();
                state.NormalizedTime = 0;
            };
        }

        private List<RootModel> GetClips(RootModel rootModel) {
            return rootModel.SelectAll<RootModel>(x => {
                var idClip = x.Select<AliasString>(x => x.Alias == AliasClip);
                var idCont = x.Select<AliasString>(x => x.Alias == AliasContainer);
                var idIsPlayed = x.Select<AliasBool>(x => x.Alias == AliasIsPlaying);
                return idClip != null && idCont != null & idIsPlayed != null;
            });
        }
    }
}