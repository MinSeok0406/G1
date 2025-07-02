
using UnityEngine;

namespace ColorPicker.InGame
{
    public class LocalBackupManager : SingletonMonobehaviour<LocalBackupManager> 
    {
        private BackupPayload backup;

        protected override void Awake()
        {
            base.Awake();
        }

        /// <summary>
        /// 호스트로 부터 데이터를 받는데 사용하는 함수
        /// </summary>
        /// <param name="payload"></param>
        public void StoreBackupFromHost(BackupPayload payload)
        {
            backup = payload;
            Debug.Log("[Backup] 데이터 저장 완료.");
        }

        public BackupPayload GetBackupPayload() => backup; // 백업 데이터 복구시 사용하는 저장되어있는 로컬 데이터 반환 함수
        public bool HasValidBackup() => backup != null;
    }
}

