import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../features/auth/application/auth_controller.dart';
import '../../features/auth/presentation/login_screen.dart';
import '../../features/auth/presentation/onboarding_screen.dart';
import '../../features/auth/presentation/register_screen.dart';
import '../../features/auth/presentation/splash_screen.dart';
import '../../features/disclaimer/presentation/disclaimer_screen.dart';
import '../../features/dreams/data/dream_models.dart';
import '../../features/dreams/presentation/analysing_screen.dart';
import '../../features/dreams/presentation/dream_details_screen.dart';
import '../../features/dreams/presentation/dream_results_screen.dart';
import '../../features/dreams/presentation/record_dream_screen.dart';
import '../../features/dreams/presentation/review_transcript_screen.dart';
import '../../features/home/presentation/home_shell.dart';
import '../../features/settings/presentation/settings_screen.dart';

/// Re-runs GoRouter's `redirect` whenever auth state changes, without
/// recreating the router (which would drop the navigation stack).
class _AuthRefreshNotifier extends ChangeNotifier {
  _AuthRefreshNotifier(Ref ref) {
    ref.listen(authControllerProvider, (_, _) => notifyListeners());
  }
}

final appRouterProvider = Provider<GoRouter>((ref) {
  final refreshNotifier = _AuthRefreshNotifier(ref);

  return GoRouter(
    initialLocation: '/splash',
    refreshListenable: refreshNotifier,
    redirect: (context, state) {
      final authState = ref.read(authControllerProvider);
      final path = state.matchedLocation;

      if (authState.isLoading) {
        return null;
      }

      final isLoggedIn = authState.valueOrNull != null;
      final isAuthRoute = path == '/login' || path == '/register' || path == '/onboarding';

      if (!isLoggedIn && !isAuthRoute) {
        return '/onboarding';
      }
      if (isLoggedIn && (isAuthRoute || path == '/splash')) {
        return '/home';
      }
      return null;
    },
    routes: [
      GoRoute(path: '/splash', name: 'splash', builder: (context, state) => const SplashScreen()),
      GoRoute(
        path: '/onboarding',
        name: 'onboarding',
        builder: (context, state) => const OnboardingScreen(),
      ),
      GoRoute(path: '/login', name: 'login', builder: (context, state) => const LoginScreen()),
      GoRoute(
        path: '/register',
        name: 'register',
        builder: (context, state) => const RegisterScreen(),
      ),
      GoRoute(path: '/home', name: 'home', builder: (context, state) => const HomeShell()),
      GoRoute(
        path: '/dreams/record',
        name: 'record-dream',
        builder: (context, state) => const RecordDreamScreen(),
      ),
      GoRoute(
        path: '/dreams/review',
        name: 'review-transcript',
        builder: (context, state) => ReviewTranscriptScreen(
          initialTranscript: state.extra as String? ?? '',
        ),
      ),
      GoRoute(
        path: '/dreams/analysing',
        name: 'analysing',
        builder: (context, state) => AnalysingScreen(dreamText: state.extra as String),
      ),
      GoRoute(
        path: '/dreams/results',
        name: 'dream-results',
        builder: (context, state) => DreamResultsScreen(result: state.extra as DreamAnalysisResult),
      ),
      GoRoute(
        path: '/dreams/:id',
        name: 'dream-details',
        builder: (context, state) => DreamDetailsScreen(dreamId: state.pathParameters['id']!),
      ),
      GoRoute(
        path: '/settings',
        name: 'settings',
        builder: (context, state) => const SettingsScreen(),
      ),
      GoRoute(
        path: '/disclaimer',
        name: 'disclaimer',
        builder: (context, state) => const DisclaimerScreen(),
      ),
    ],
  );
});
