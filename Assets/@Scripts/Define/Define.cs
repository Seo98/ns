using UnityEngine;

public enum GamePhase
{
    Exploration,    // 자유 탐색
    RuleTriggered,  // 규칙 발동
    RuleExecution,  // 규칙 실행 중
    Consequence     // 결과 처리
}

public enum FloorStay
{
    FIRSTFLOOR,
    SECONDFLOOR,
    THIRDFLOOR,
}
