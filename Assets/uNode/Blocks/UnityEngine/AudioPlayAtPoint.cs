using UnityEngine;

namespace MaxyGames.Events {
	[BlockMenu("UnityEngine", "Audio.PlayAtPoint")]
	public class AudioPlayAtPoint : Action {
		public bool useTransform = true;
		[Hide("useTransform", false)]
		[ObjectType(typeof(Transform))]
		public MemberData transform;
		[Hide("useTransform", true)]
		[ObjectType(typeof(Vector3))]
		public MemberData position;
		public bool playRandomSound;
		[Hide("playRandomSound", true)]
		[ObjectType(typeof(AudioClip))]
		public MemberData clip;
		[Hide("playRandomSound", false)]
		[ObjectType(typeof(AudioClip))]
		public MemberData[] clips;
		[ObjectType(typeof(float))]
		public MemberData volumeScale = new MemberData(1);

		protected override void OnExecute() {
			if(useTransform) {
				if(!playRandomSound) {
					AudioSource.PlayClipAtPoint(clip.Get<AudioClip>(), transform.Get<Transform>().position, volumeScale.Get<float>());
				} else if(clips.Length > 0) {
					AudioSource.PlayClipAtPoint(clips[Random.Range(0, clips.Length - 1)].Get<AudioClip>(), transform.Get<Transform>().position, volumeScale.Get<float>());
				}
			} else {
				if(!playRandomSound) {
					AudioSource.PlayClipAtPoint(clip.Get<AudioClip>(), position.Get<Vector3>(), volumeScale.Get<float>());
				} else if(clips.Length > 0) {
					AudioSource.PlayClipAtPoint(clips[Random.Range(0, clips.Length - 1)].Get<AudioClip>(), position.Get<Vector3>(), volumeScale.Get<float>());
				}
			}
		}

		public override string GenerateCode(Object obj) {
			string position;
			if(useTransform) {
				position = CG.Value((object)transform).Add(".position");
			} else {
				position = CG.Value((object)this.position);
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
					data += CG.FlowInvoke(typeof(AudioSource), "PlayClipAtPoint",
						CG.Invoke(varName + "[]",  CG.Invoke(typeof(Random), "Range", CG.Value(0), CG.Value(clips.Length - 1))),
						position,
						CG.Value(volumeScale)).AddLineInFirst();
					return data;
				} else {
					throw new System.Exception("The clips is empty");
				}
			}
			return CG.FlowInvoke(typeof(AudioSource), "PlayClipAtPoint", clip.CGValue(), position, volumeScale.CGValue());
		}

		public override void CheckError(Object owner) {
			base.CheckError(owner);
			uNode.uNodeUtility.CheckError(volumeScale, owner, Name + " - volumeScale");
			if(!playRandomSound) {
				uNode.uNodeUtility.CheckError(clip, owner, Name + " - clip");
			} else {
				uNode.uNodeUtility.CheckError(clips, owner, Name + " - clip");
			}
			if(useTransform) {
				uNode.uNodeUtility.CheckError(transform, owner, Name + " - transform");
			} else {
				uNode.uNodeUtility.CheckError(position, owner, Name + " - position");
			}
		}

		public override string GetDescription() {
			return "Plays an AudioClip at a given position in world space.";
		}
	}
}