using UnityEngine;

namespace CorePresenter.UltEventExtension.Helper
{
    [AddComponentMenu("MV*/Event mediator/Helper-Math")]
    public class MathUlt : MonoBehaviour
    {
        public float Add(float a, float b) => a + b;
        public float Sub(float a, float b) => a - b;
        public float Mul(float a, float b) => a * b;
        public float Div(float a, float b) => a / b;
        public float Mod(float a, float b) => a % b;
        public float Invert(float a) => a * -1;
        
        public float Add(float a, int b) => a + b;
        public float Sub(float a, int b) => a - b;
        public float Mul(float a, int b) => a * b;
        public float Div(float a, int b) => a / b;
        public float Mod(float a, int b) => a % b;
        
        public float Add(int a, float b) => a + b;
        public float Sub(int a, float b) => a - b;
        public float Mul(int a, float b) => a * b;
        public float Div(int a, float b) => a / b;
        public float Mod(int a, float b) => a % b;
        
        public int Add(int a, int b) => a + b;
        public int Sub(int a, int b) => a - b;
        public int Mul(int a, int b) => a * b;
        public int Div(int a, int b) => a / b;
        public int Mod(int a, int b) => a % b;
        public int Invert(int a) => a * -1;
        
        public Vector3 Add(Vector3 a, Vector3 b) => a + b;
        public Vector3 Sub(Vector3 a, Vector3 b) => a - b;
        
        public Vector2 Add(Vector2 a, Vector2 b) => a + b;
        public Vector2 Sub(Vector2 a, Vector2 b) => a - b;
        public Vector2 Mul(Vector2 a, Vector2 b) => a * b;
        public Vector2 Div(Vector2 a, Vector2 b) => a / b;

        public bool Invert(bool a) => !a;

        public Vector3 Cast(Vector2 v) => v;
        public Vector2 Cast(Vector3 v) => v;
    }
}