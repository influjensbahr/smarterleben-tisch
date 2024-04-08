using Sparrow.Verification;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OTBT.Framework.Visuals
{
    public class ParticleEffectPool : MonoBehaviour, IVerify
    {
        [SerializeField] ParticleSystem m_ParticleTemplate;
        [SerializeField] int m_MaxParticles = 6;

        Queue<ParticleSystem> m_Pool;

        Transform m_ParticleParent;

        void Awake()
        {
            var parent = new GameObject($"Pool - {m_ParticleTemplate.name}");
            parent.transform.SetParent(transform);
            m_ParticleParent = parent.transform;

            InitializePool();
        }

        void InitializePool()
        {
            m_Pool = new Queue<ParticleSystem>(m_MaxParticles);
            for (int i = 0; i < m_MaxParticles; i++)
            {
                var go = Instantiate(m_ParticleTemplate.gameObject, m_ParticleParent);
                go.SetActive(false);

                var particle = go.GetComponent<ParticleSystem>();
                m_Pool.Enqueue(particle);
            }
        }

        public void Play(Vector3 position)
        {
            if (m_Pool == null) InitializePool();
            if (m_Pool.Count <= 0)
            {
                Debug.LogWarning("Could not get a Particle from the Pool. Consider Raising Pool Size.", this);
                return;
            }
            var particle = m_Pool.Dequeue();

            particle.gameObject.SetActive(true);
            particle.gameObject.transform.position = position;
            particle.Play();

            StartCoroutine(ReturnToPoolAfter(particle, particle.main.duration));
        }

        IEnumerator ReturnToPoolAfter(ParticleSystem particle, float duration)
        {
            yield return new WaitForSeconds(duration);

            particle.gameObject.SetActive(false);
            m_Pool.Enqueue(particle);
        }


        public void Verify(CheckVerifyInterface checker)
        {
            checker.Check(m_ParticleTemplate != null, "ParticleEffectPool has no Template", gameObject);
        }
    }
}
