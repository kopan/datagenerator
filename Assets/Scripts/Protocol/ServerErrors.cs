
public enum HTTP_Error_Code
{
	None = 0,

	NetworkError,       // 물리적 네트워크 연결불능, 타임아웃 

	ErrorFromServer,    // 게임 로직상 오류 (다이아부족 등 Client/Server 상태 불일치 가능)

	BadRequet,

	SystemError,        // 서버 데이타 오류, 계정 블록 (계정 생성 실패, 로그인 금지 등 CS차원에서 대응)

	LogicErrors = 1000,     // (err > LogicErrors) => PokoCode. 

}


public enum ServerError {
	UnknownError 	= -3,
	ServerDataError = -2,	// JSON,
	NetworkError	= -1,	// Timeout, BadRequest, NotConnect
	Success			= 0,
	
	AuthError		= 1,
	TimeoutTs		= 2,	// Request문서의 만료
	AuthDuplicated	= 3,
	AlreadyExists	= 4,
	Expired			= 5,
	OverLimit		= 6,
	Ban				= 7,
	
	NotFoundDB		= 10,
	ErrorDB			= 11,
	OverflowAsset	= 12,

	LackAsset		= 100,
	LackClover		= 101,
	LackCoin		= 102,
	LackStar		= 103,
	LackJewel		= 104,
	LackWing		= 105,

	ExpiredTime		= 200,
	Inactive        = 201,
	
	InvalidRequest	= 900,
	ServerInternal	= 999,
	
	// Error From Server
}
