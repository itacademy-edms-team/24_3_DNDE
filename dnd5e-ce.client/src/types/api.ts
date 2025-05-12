// Интерфейс для ошибок валидации от сервера export interface IValidationError {   code: string;   description: string; }  export interface IContainsAccessToken {   accessToken: string; }  export interface IContainsRefreshToken {   refreshToken: string;
}

export interface IContainsTokens extends IContainsAccessToken, IContainsRefreshToken { }