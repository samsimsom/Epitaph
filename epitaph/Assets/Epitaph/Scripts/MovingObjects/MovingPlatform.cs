using System.Collections;
using UnityEngine;

namespace Epitaph.Scripts.MovingObjects
{
    public class MovingPlatform : MonoBehaviour
    {
        [Header("Platform Settings")]
        [SerializeField] private float waitTime = 3f;
        [SerializeField] private float moveDistance = 10f;
        [SerializeField] private float moveSpeed = 5f;
        
        [Header("Components")]
        [SerializeField] private Rigidbody rb;
        
        private Vector3 _startPosition;
        private bool _isMoving = false;
        
        private void Start()
        {
            // Başlangıç pozisyonunu kaydet
            _startPosition = transform.position;
            
            // Rigidbody yoksa otomatik olarak al
            if (rb == null)
                rb = GetComponent<Rigidbody>();
            
            // Rigidbody ayarları
            if (rb != null)
            {
                rb.isKinematic = true; // Fizik etkileşimlerini engelle
            }
            
            // Hareketi başlat
            StartCoroutine(MovementSequence());
        }
        
        private IEnumerator MovementSequence()
        {
            while (true)
            {
                // 1. Başlangıçta bekle
                yield return new WaitForSeconds(waitTime);
                
                // 2. Yukarı çık
                yield return StartCoroutine(MoveTo(_startPosition + Vector3.up * moveDistance));
                yield return new WaitForSeconds(waitTime);
                
                // 3. İleri git
                yield return StartCoroutine(MoveTo(_startPosition + Vector3.up * moveDistance + Vector3.forward * moveDistance));
                yield return new WaitForSeconds(waitTime);
                
                // 4. Aşağı in
                yield return StartCoroutine(MoveTo(_startPosition + Vector3.forward * moveDistance));
                yield return new WaitForSeconds(waitTime);
                
                // 5. Başlangıç noktasına dön
                yield return StartCoroutine(MoveTo(_startPosition));
            }
        }
        
        private IEnumerator MoveTo(Vector3 targetPosition)
        {
            _isMoving = true;
            var startPos = transform.position;
            var journey = 0f;
            var totalDistance = Vector3.Distance(startPos, targetPosition);
            
            while (journey <= totalDistance)
            {
                journey += moveSpeed * Time.deltaTime;
                var fractionOfJourney = journey / totalDistance;
                
                // Smooth movement using Lerp
                var newPosition = Vector3.Lerp(startPos, targetPosition, fractionOfJourney);
                
                if (rb != null)
                {
                    rb.MovePosition(newPosition);
                }
                else
                {
                    transform.position = newPosition;
                }
                
                yield return null;
            }
            
            // Hedef pozisyona tam olarak yerleştir
            if (rb != null)
            {
                rb.MovePosition(targetPosition);
            }
            else
            {
                transform.position = targetPosition;
            }
            
            _isMoving = false;
        }
        
        // Platform üzerindeki nesneleri birlikte taşımak için
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                other.transform.SetParent(transform);
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                other.transform.SetParent(null);
            }
        }
        
        // Debug için Gizmos
        private void OnDrawGizmos()
        {
            var basePos = Application.isPlaying ? _startPosition : transform.position;
            
            // Hareket yolunu çiz
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(basePos, Vector3.one * 0.5f); // Başlangıç
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(basePos + Vector3.up * moveDistance, Vector3.one * 0.5f); // Yukarı
            
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(basePos + Vector3.up * moveDistance + Vector3.forward * moveDistance, Vector3.one * 0.5f); // İleri
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(basePos + Vector3.forward * moveDistance, Vector3.one * 0.5f); // Aşağı
            
            // Hareket çizgilerini çiz
            Gizmos.color = Color.white;
            Gizmos.DrawLine(basePos, basePos + Vector3.up * moveDistance);
            Gizmos.DrawLine(basePos + Vector3.up * moveDistance, basePos + Vector3.up * moveDistance + Vector3.forward * moveDistance);
            Gizmos.DrawLine(basePos + Vector3.up * moveDistance + Vector3.forward * moveDistance, basePos + Vector3.forward * moveDistance);
            Gizmos.DrawLine(basePos + Vector3.forward * moveDistance, basePos);
        }
    }
}