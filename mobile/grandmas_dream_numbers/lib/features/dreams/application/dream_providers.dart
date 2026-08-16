import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../data/dream_models.dart';
import '../data/dreams_api.dart';

final dreamHistoryProvider = FutureProvider.autoDispose.family<List<DreamSummary>, String?>(
  (ref, query) => ref.watch(dreamsApiProvider).getHistory(query: query),
);

final dreamDetailProvider = FutureProvider.autoDispose.family<DreamAnalysisResult, String>(
  (ref, dreamId) => ref.watch(dreamsApiProvider).getDetail(dreamId),
);

final dreamSubmissionProvider = FutureProvider.autoDispose.family<DreamAnalysisResult, String>(
  (ref, dreamText) => ref.watch(dreamsApiProvider).analyse(dreamText),
);

final dreamReanalysisProvider = FutureProvider.autoDispose.family<DreamAnalysisResult, String>(
  (ref, dreamId) => ref.watch(dreamsApiProvider).analyseAgain(dreamId),
);
