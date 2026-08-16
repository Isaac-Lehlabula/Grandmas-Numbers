class UserProfile {
  const UserProfile({
    required this.id,
    required this.firstName,
    required this.email,
    required this.createdAt,
  });

  final String id;
  final String firstName;
  final String email;
  final DateTime createdAt;

  factory UserProfile.fromJson(Map<String, dynamic> json) => UserProfile(
    id: json['id'] as String,
    firstName: json['firstName'] as String,
    email: json['email'] as String,
    createdAt: DateTime.parse(json['createdAt'] as String),
  );
}

class AuthSession {
  const AuthSession({
    required this.accessToken,
    required this.accessTokenExpiresAt,
    required this.refreshToken,
    required this.refreshTokenExpiresAt,
    required this.user,
  });

  final String accessToken;
  final DateTime accessTokenExpiresAt;
  final String refreshToken;
  final DateTime refreshTokenExpiresAt;
  final UserProfile user;

  factory AuthSession.fromJson(Map<String, dynamic> json) => AuthSession(
    accessToken: json['accessToken'] as String,
    accessTokenExpiresAt: DateTime.parse(json['accessTokenExpiresAt'] as String),
    refreshToken: json['refreshToken'] as String,
    refreshTokenExpiresAt: DateTime.parse(json['refreshTokenExpiresAt'] as String),
    user: UserProfile.fromJson(json['user'] as Map<String, dynamic>),
  );
}
