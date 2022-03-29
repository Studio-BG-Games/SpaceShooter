using UnityEngine;

namespace CorePresenter.UltEventExtension.Helper
{
    [AddComponentMenu("MV*/Event mediator/Helper-Bool")]
    public class BoolUlt : MonoBehaviour
    {
        //Float
        public bool More(float a, float b) => a > b;
        public bool Less(float a, float b) => a < b;
        public bool LessEquals(float a, float b) => a <= b;
        public bool MoreEquals(float a, float b) => a >= b;
        public bool Equals(float a, float b) => a == b;
        
        //Int
        public bool More(int a, int b) => a > b;
        public bool Less(int a, int b) => a < b;
        public bool LessEquals(int a, int b) => a <= b;
        public bool MoreEquals(int a, int b) => a >= b;
        public bool Equals(int a, int b) => a == b;
        
        //String
        public bool Equals(string a, string b) => a == b;
        
        //Vector3
        public bool Equals(Vector3 a, Vector3 b) => a == b;
        
        //Vector2
        public bool Equals(Vector2 a, Vector2 b) => a == b;
        
        // collider2d
        public bool IsTrigger(Collider2D collider2D) => collider2D.isTrigger;
    }
}