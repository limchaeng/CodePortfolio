# 컨텐츠 코드 스타일 예제
## [battle_system](battle_system) : 스킬 시스템
 * 전투 스킬 관련 코드
 * 모든 스킬은 [SkillBase.cs](battle_system/SkillBase.cs) 를 상속 받고 있으며, 특징 및 시전, 타겟, 순서, 조건에 따라 스킬 발동을 다르게 처리함이 목적.
## [common](common) : 공통사용 모듈
## [contents](contents) : 컨텐츠 관련
 * [CSRewardData.cs](contents/CSRewardData.cs)
   * 보상 공통 모듈
   * 전체 시스템의 모든 보상구조를 통일하여 기획 단계 부터, UI, 서버처리, DB 저장까지 쉽게 적용할수 있게 구조 설계.
   * 모든 보상 관련 처리를 한 곳에서 관리 및 처리할수 있어 편리하게 사용됨.
  * [PlayerAchievementData.cs](contents/PlayerAchievementData.cs)
    * PlayerData 상속
      * 클라이언트의 여러 플레이어 데이터(업적, 미션, 각종 스텟등..) 등을 관리.
     * 예제는 업적관련 코드
     * 플레이어 데이터는 클라이언트와 서버에서 같은 동작을 하는 부분이 많기에 C# 서버 사용시에 중복 작업을 없애고 코드를 공유해서 한번의 작업으로 클라/서버를 처리하게 구조를 설계함.  
## [unity](unity) : 유니티 코드
 * [FXPlayBase.cs](unity/FXPlayBase.cs)
   * 유니티 파티클 플레이 처리 : Cast, Hit, Projectile 타입의 파티클 처리를 이 클래스를 상속받아서 확장 구현.
   * [FXToolEditorWindow.cs](unity/Editor/FXToolEditorWindow.cs)
     * 제작된 FX의 플레이 테스트, 마이그레이션, 오류체크등을 위한 에디터툴 코드
 * [I18NTextBase.cs](unity/I18NTextBase.cs)
   * I18N text 처리 기본 : 키를 기준으로 런타임시 언어별 텍스트 처리
   * 런타임시 폰트 변경 및 실시간 언어 변환 처리
 
