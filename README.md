## Code Portfolio
> 이 사이트는 개발 코드 포트폴리오를 위해 작성 되었습니다.
### Unified Mobile Framework (UMF)
> * 구현 언어 : C#
> * <b>개요</b> : 서버/클라이언트 구조의 새로운 프로젝트 진행시 이전 프로젝트에서 사용하던 기본적인 기능이 반복적으로 구현이 되는 경우가 많아 이를 최소화 하여 개발 시간을 단축하고, 자주 쓰는 코드의 공용화를 위해 개인적으로 구현 하고 있는 프로젝트입니다.
> * 유니티 클라이언트에서 사용하는 <b>UMF.Unity</b> 는 현재도 프로젝트 개발에 사용중이며 자주 쓰는 기능 및 UI 관련 확장 기능 들을 계속 추가하고 있습니다.
* ### 핵심기능 
	* [UMF](UMF)
		* [UMF.Core](UMF/UMF.Core)
			* 프로젝트들의 필수 메인 코어 모듈.
			* Config, 기획 테이블 관리, 로컬라이징 텍스트 관리, 기능모듈등 공통 코드 구현.
		* [UMF.Database](UMF/UMF.Database)
			* 서버에서 사용하는 데이터베이스 코어 모듈.
			* MSSql, MySql 지원, Procedure 기반의 DB 처리 모듈.
		* [UMF.DeltaPatch](UMF/UMF.DeltaPatch)
			* 델타패치를 지원하기 위한 프로젝트.(클라이언트 및 툴에서 사용).
		* [UMF.Net](UMF/UMF.Net)
			* 네트워크 코어 모듈.
			* C# 기반, Parallel 지원, 패킷 처리.
		* [UMF.Server](UMF/UMF.Server)
			* 서버 코어 모듈
			* C# 기반 서버 어플리케이션 기본 코드.
	* [UMF.Unity](UMF.Unity)
		* Unity 에서 사용. Package 형식으로 사용할수 있게 구현.
		* 플렛폼 별 커스텀 빌드 지원.
		* 자주사용하는 Inspector Draw 관련 기능 : Drag and drop, layout 조정되는 button list 등..
		* Singleton, Prefab 관리, 인게임 화면, 팝업 관리등의 기본 클래스 및 매니저 클래스.
		* Fade in/out, Loop 지원 사운드 플레이, 배경 음악 관리등.
		* UI 및 Component 확장 유틸.
	* [UMP](UMP)
		* UMF 코어 모듈을 사용하여 기본적인 서버 구조의 프로토타입 구현 및 테스트 환경 제공.
		* 기본형 서버(템플릿)들을 구현하고, 클라이언트 연동 및 구현한 프레임워크, 모듈등을 테스트 하기 위해 작성.
  	* [UMTools](UMTools) 
		* C# WinForm 으로 구현된 개발용 툴
		* 하나의 툴로 여러 프로젝트에서 사용 가능하게 설계됨.
		* [UMDistribution](UMTools/UMDistribution)
    		* 개발중 자주쓰지만 조작이 귀찮은 것들을 버튼으로 빠르게 실행하기 위한 툴.
  		* [UMLauncher](UMTools/UMLauncher)
    		* 개발용 빌드의 심플한 런처 툴.
    		* 클라이언트/서버(Windows 빌드)를 로컬에서 실행.
    		* 자동 업데이트 지원, 버전별 실행 지원.
  		* [UMTBLExport](UMTools/UMTBLExport)
    		* 엑셀로 제작된 기획 데이터를 게임내에서 사용하기 위한 테이블 Export 툴.
    		* 하나의 엑셀에서 게임용 데이터와 로컬라이징 텍스트를 분리하여 추출.
    		* 번역 관리 지원. 번역 파일 Export/Import 기능.
### 웹서비스 구현
> * 구현 ASP.NET
> * 프로젝트 진행중 웹콜백 서비스 및 웹 API 기능이 필요하여 ASP.NET 으로 구현했던 코드
* [Web Callback](Web/Callback/Controllers/QKController.cs)
  * 중국서비스시 QuickSDK 사용 결제시 웹으로 결제에 대한 콜백 데이터를 받아야 해서 구현함.
  * 결제후 외부서버에서 콜백을 받아 자체 DB에 정보를 저장하고 게임서버에서 API 를 호출해 결제 검증.
* [Web Tool](Web/Pages/Wemix)
  * 게임 서비스 중 개발한 내부에서 사용한 웹툴.
  * 게임로그뷰어, 데이터수정등 운영에 필요한 전반적인 기능 구현.
  * 그 중 일부인 <b>위믹스 에어 드랍</b>을 하기 위한 부분만 첨부.
### 컨텐츠 코드 샘플
* [excodefile](excodefile)
