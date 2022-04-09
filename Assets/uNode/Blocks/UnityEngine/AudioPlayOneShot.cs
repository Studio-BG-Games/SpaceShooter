using UnityEngine;

namespace MaxyGames.Events {
	[BlockMenu("UnityEngine", "Audio.PlayOneShot")]
	public class AudioPlayOneShot : Action {
		[ObjectType(typeof(AudioSource))]
		public MemberData audio;
		public bool playRandomSound;
		[Hide("playRandomSound", true)]
		[ObjectType(typeof(AudioClip))]
		public MemberData clip;
		[Hide("playRandomSound", false)]
		[ObjectType(typeof(AudioClip))]
		public MemberData[] clips;
		[ObjectType(typeof(float))]
		public MemberData volumeScale;

		protected override void OnExecute() {
				if(audio == null) {
					Debug.LogError("No Audio Source");
					return;
				}
				AudioSource source = audio.Get<AudioSource>();
				if(source != null) {
					if(!playRandomSound) {
						source.PlayOneShot(clip.Get<AudioClip>(), volumeScale.Get<float>());
						return;
					}
					if(clips.Length > 0) {
						source.PlayOneShot(clips[Random.Range(0, clips.Length - 1)].Get<AudioClip>(), volumeScale.Get<float>());
					}
				}
		}

		public override string GenerateCode(Object obj) {
			string audioName = null;
			if(audio.isAssigned) {
				audioName = CG.Value((object)audio);
			} else {
				return null;
			}
			if(playRandomSound) {
				string data = null;
				if(clips.Length > 0) {
					string varName = CG.GenerateVariableName("random", this);
					string randomData = CG.DeclareVariable(varName, typeof(System.Collections.Generic.List<AudioClip>));
					data += randomData;
					foreach(var var in clips) {
						data += CG.FlowInvoke(varName, "Add", var.CGValue()).AddLineInFirst();
					}
					data += CG.FlowInvoke(audioName, "PlayOneShot",
						CG.Invoke(varName + "[]",  CG.Invoke(typeof(Random), "Range", CG.Value(0), CG.Value(clips.Length - 1))),
						CG.Value(volumeScale)).AddLineInFirst();
					return data;
				}
			}
			return CG.FlowInvoke(audioName, "PlayOneShot", clip.CGValue(), volumeScale.CGValue());
		}

		public override void CheckError(Object owner) {
			base.CheckError(owner);
			uNode.uNodeUtility.CheckError(audio, owner, Name + " - audio");
			uNode.uNodeUtility.CheckError(volumeScale, owner, Name + " - volumeScale");
			if(!playRandomSound) {
				uNode.uNodeUtility.CheckError(clip, owner, Name + " - clip");
			} else {
				uNode.uNodeUtility.CheckError(clips, owner, Name + " - clip");
			}
		}

		public override string GetDescription() {
			return "Plays an AudioClip, and scales the AudioSource volume by volumeScale.";
		}
	}
}