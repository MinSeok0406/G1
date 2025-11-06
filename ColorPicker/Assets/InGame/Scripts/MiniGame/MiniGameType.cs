namespace ColorPicker.InGame
{
    public enum MiniGameType
    {
        None = 0,

        // === Drag 기반 미니게임 ===
        Clean = 1,              // 청소하기 (기존)
        ToySort = 2,            // 장난감 정리
        CardSwipe = 3,          // 카드 긁기
        WireConnect = 4,        // 전선 연결
        FuelEngine = 5,         // 연료 주입
        AlignEngine = 6,        // 엔진 정렬 (슬라이더)
        UnlockManifolds = 7,    // 자물쇠 열기 (숫자 맞추기)
        EmptyGarbage = 8,       // 쓰레기 버리기
        SortSamples = 9,        // 샘플 분류

        // === Click 기반 미니게임 ===
        ButtonSequence = 10,    // 버튼 순서 누르기
        NumberPad = 11,         // 숫자 패드 입력
        ShootAsteroids = 12,    // 운석 슈팅 (레트로 슈팅)
        FixWiring = 13,         // 배선 수리 (스위치)

        // === Click + Time 기반 미니게임 ===
        Download = 14,          // 다운로드 대기
        ScanBody = 15,          // 몸 스캔 대기

        // === 정신병원 테마 미니게임 ===
        // Drag 기반
        PillSort = 20,          // 약물 분류 (색상/타입별)
        PatientFileSort = 21,   // 환자 기록 정리 (서류 정리)
        SedativeInjection = 22, // 진정제 주사 (주사기 드래그)
        StraitjacketTie = 23,   // 구속복 묶기 (끈 연결)
        BrainWavePattern = 24,  // 뇌파 패턴 맞추기 (슬라이더)

        // Click 기반
        HeartRateMonitor = 25,  // 심박수 모니터링 (비정상 클릭)
        MentalStateExam = 26,   // 정신 상태 검사 (질문 응답)
        SecurityCamera = 27,    // 감시 카메라 확인 (이상 발견)
        DiagnosisForm = 28,     // 진단서 작성 (체크리스트)

        // Click + Time 기반
        MedicineSchedule = 29,  // 투약 시간 체크 (타이머)
    }
}
