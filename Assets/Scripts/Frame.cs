using UnityEngine;

namespace ReplayData
{
    public class Frame
    {
        private Vector3 position;
        private Quaternion rotation;
        private Vector3 scale;
        private Vector3 rbVelocity;
        private Vector3 rbAngularVelocity;
        private float particleTime;

        public Frame(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }

        public void SetRBVelocities(Vector3 velocity, Vector3 angularVelocity)
        {
            rbVelocity = velocity;
            rbAngularVelocity = angularVelocity;
        }

        public void SetParticleTime(float time)
        {
            particleTime = time;
        }

        public Vector3 GetPosition() => position;
        public Quaternion GetRotation() => rotation;
        public Vector3 GetScale() => scale;
        public Vector3 GetRBVelocity() => rbVelocity;
        public Vector3 GetRBAngularVelocity() => rbAngularVelocity;
        public float GetParticleTime() => particleTime;
    }
}