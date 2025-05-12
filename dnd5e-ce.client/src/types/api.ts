// Интерфейс для ошибок валидации от сервера export interface IValidationError {   code: string;   description: string; }  export interface IContainsAccessToken {   accessToken: string; }  export interface IContainsRefreshToken {   refreshToken: string; }  export interface IContainsTokens extends IContainsAccessToken, IContainsRefreshToken { }  export type LoginData = {
  email: string;
  password: string;
};

export interface IRegisterFormData {
  username: string;
  email: string;
  password: string;
  passwordConfirm: string;
}  export interface ILoginFormData {
  email: string;
  password: string;
}  export interface AuthResponse {   status: boolean;
  errors: string[];
}