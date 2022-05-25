https://console.developers.google.com/ 에 접속

데이터제네레이터는 기획자가 구글 스프레드시트에서 작성한 각종 데이터를 클라이언트, 서버가 편리하게 사용할 수 있도록 c# 코드와 xml 로 생성해주는 unity에서 돌아가는 도구이다.

데이터제네레이터 프로젝트를 하나 만들고 google drive api 와 google sheets api 를 추가 해준다.

사용자인증정보 탭에서 OAuth 2.0 클라이언트 ID 를 만들고 json 파일로 받아서

Assets\Resources\Config 폴더에 credentials_service.json 이름으로 저장 한다.

구글 드라이브에서 폴더를 하나 만든다. (개인 드라이브는 소유자가 탈퇴시 파일이 날아가므로 공유 드라이브에 만들기 권장)

datagenertor의 sample 폴더안에 있는 xlsx 파일들을 올린다

올린 파일을 오른쪽 클릭→ 연결앱→ 구글 스프레드 시트로 연다.

파일→저장하기로 저장한다.

모두 완료후 xlsx 파일들은 지운다.

데이터 생성법

소스트리로 datagenerator를 받아서 유니티로 연다

Tools→Datagenerator 를 선택후 뜨는 창에서 DriveFolder 네임에 구글드라이브의 폴더 이름을 적는다.

Generate 를 누르면 생성 시작

DriveFileName Prefix	해당 이름이 들어간 파일만 생성
ClientPath	데이터 생성후 복사될 클라이언트 위치
ServerPath	데이터 생성후 복사될 서버 위치
Force ReGenerate ALL	강제로 데이터를 모두 재생성
Copy Only	클라,서버 데이터 복사만 실행
