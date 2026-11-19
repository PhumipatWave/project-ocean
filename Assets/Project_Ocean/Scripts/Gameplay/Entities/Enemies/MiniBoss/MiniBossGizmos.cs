using UnityEngine;

namespace Game.Gameplay
{
    /// ช่วยดูขอบเขตของบอสในหน้า Scene เท่านั้น ไม่มีผลต่อเกม
    public class MiniBossGizmos : MonoBehaviour
    {
        [SerializeField] private MiniBossProfile profile;

        private Vector3 home;

        private void OnEnable() => home = transform.position;

        private void OnDrawGizmosSelected()
        {
            if (profile == null) return;

            Vector3 center = Application.isPlaying ? home : transform.position;

            // วงแดง: ขอบเขต Leash (ออกนอกวงนี้บอสว่ายกลับ)
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(center, profile.leashRadius);

            // วงเหลือง: รัศมีเดินเล่น
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(center, profile.wanderRadius);

            // วงฟ้า: ระยะยิงของแต่ละสกิล (วาดรอบตัวบอส)
            Gizmos.color = Color.cyan;
            foreach (var slot in profile.skills)
                if (slot.data != null)
                    Gizmos.DrawWireSphere(transform.position, slot.data.maxRange);

            // เส้นจากบ้านถึงบอส: เขียว = ในเขต, แดง = เกินเขต
            bool outside = Vector3.Distance(center, transform.position) > profile.leashRadius;
            Gizmos.color = outside ? Color.red : Color.green;
            Gizmos.DrawLine(center, transform.position);
        }
    }
}