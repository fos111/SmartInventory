import { ConverterError } from './errors';

// Result type used by converter operations
export type Result<T> = { ok: true; value: T } | { ok: false; error: ConverterError };
