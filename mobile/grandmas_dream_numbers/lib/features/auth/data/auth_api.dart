import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/network/api_client.dart';
import 'auth_models.dart';

final authApiProvider = Provider<AuthApi>(
  (ref) => AuthApi(ref.watch(apiClientProvider).dio),
);

class AuthApi {
  AuthApi(this._dio);

  final Dio _dio;

  Future<AuthSession> register({
    required String firstName,
    required String email,
    required String password,
  }) async {
    final response = await _dio.post(
      '/auth/register',
      data: {'firstName': firstName, 'email': email, 'password': password},
    );
    return AuthSession.fromJson(response.data as Map<String, dynamic>);
  }

  Future<AuthSession> login({required String email, required String password}) async {
    final response = await _dio.post(
      '/auth/login',
      data: {'email': email, 'password': password},
    );
    return AuthSession.fromJson(response.data as Map<String, dynamic>);
  }

  Future<void> logout(String refreshToken) =>
      _dio.post('/auth/logout', data: {'refreshToken': refreshToken});

  Future<UserProfile> getProfile() async {
    final response = await _dio.get('/profile');
    return UserProfile.fromJson(response.data as Map<String, dynamic>);
  }
}
