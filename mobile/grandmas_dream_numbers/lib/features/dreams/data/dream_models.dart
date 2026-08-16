class DreamSymbolMatch {
  const DreamSymbolMatch({
    required this.symbolId,
    required this.symbolName,
    required this.matchedText,
    required this.traditionalMeaning,
    required this.explanation,
    required this.confidenceScore,
    required this.luckyNumbers,
  });

  final String symbolId;
  final String symbolName;
  final String matchedText;
  final String traditionalMeaning;
  final String explanation;
  final double confidenceScore;
  final List<int> luckyNumbers;

  factory DreamSymbolMatch.fromJson(Map<String, dynamic> json) => DreamSymbolMatch(
    symbolId: json['symbolId'] as String,
    symbolName: json['symbolName'] as String,
    matchedText: json['matchedText'] as String,
    traditionalMeaning: json['traditionalMeaning'] as String,
    explanation: json['explanation'] as String,
    confidenceScore: (json['confidenceScore'] as num).toDouble(),
    luckyNumbers: (json['luckyNumbers'] as List).map((n) => n as int).toList(),
  );
}

class SuggestedCombination {
  const SuggestedCombination({required this.numbers, required this.combinationType});

  final List<int> numbers;
  final String combinationType;

  factory SuggestedCombination.fromJson(Map<String, dynamic> json) => SuggestedCombination(
    numbers: (json['numbers'] as List).map((n) => n as int).toList(),
    combinationType: json['combinationType'] as String,
  );
}

class DreamAnalysisResult {
  const DreamAnalysisResult({
    required this.dreamId,
    required this.dreamAnalysisId,
    required this.originalText,
    required this.summary,
    required this.matches,
    required this.suggestedCombinations,
    required this.disclaimer,
    required this.createdAt,
  });

  final String dreamId;
  final String dreamAnalysisId;
  final String originalText;
  final String summary;
  final List<DreamSymbolMatch> matches;
  final List<SuggestedCombination> suggestedCombinations;
  final String disclaimer;
  final DateTime createdAt;

  factory DreamAnalysisResult.fromJson(Map<String, dynamic> json) => DreamAnalysisResult(
    dreamId: json['dreamId'] as String,
    dreamAnalysisId: json['dreamAnalysisId'] as String,
    originalText: json['originalText'] as String,
    summary: json['summary'] as String,
    matches: (json['matches'] as List)
        .map((m) => DreamSymbolMatch.fromJson(m as Map<String, dynamic>))
        .toList(),
    suggestedCombinations: (json['suggestedCombinations'] as List)
        .map((c) => SuggestedCombination.fromJson(c as Map<String, dynamic>))
        .toList(),
    disclaimer: json['disclaimer'] as String,
    createdAt: DateTime.parse(json['createdAt'] as String),
  );
}

class DreamSummary {
  const DreamSummary({
    required this.id,
    required this.originalText,
    required this.summary,
    required this.createdAt,
    required this.hasAnalysis,
  });

  final String id;
  final String originalText;
  final String? summary;
  final DateTime createdAt;
  final bool hasAnalysis;

  factory DreamSummary.fromJson(Map<String, dynamic> json) => DreamSummary(
    id: json['id'] as String,
    originalText: json['originalText'] as String,
    summary: json['summary'] as String?,
    createdAt: DateTime.parse(json['createdAt'] as String),
    hasAnalysis: json['hasAnalysis'] as bool,
  );
}
