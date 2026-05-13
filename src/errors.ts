/**
 * Base error class for all converter errors.
 */
export class ConverterError extends Error {
  constructor(message: string, public readonly context?: Record<string, unknown>) {
    super(message);
    this.name = 'ConverterError';
  }
}

/**
 * Error thrown when SVG parsing fails.
 */
export class SvgParseError extends ConverterError {
  constructor(message: string, context?: Record<string, unknown>) {
    super(message, context);
    this.name = 'SvgParseError';
  }
}

/**
 * Error thrown when PlantUML parsing fails.
 */
export class PlantumlParseError extends ConverterError {
  constructor(message: string, context?: Record<string, unknown>) {
    super(message, context);
    this.name = 'PlantumlParseError';
  }
}

/**
 * Error thrown when DGM.js JSON generation fails.
 */
export class DgmjsGenerationError extends ConverterError {
  constructor(message: string, context?: Record<string, unknown>) {
    super(message, context);
    this.name = 'DgmjsGenerationError';
  }
}

/**
 * Error thrown when input/output validation fails.
 */
export class ValidationError extends ConverterError {
  constructor(message: string, context?: Record<string, unknown>) {
    super(message, context);
    this.name = 'ValidationError';
  }
}
