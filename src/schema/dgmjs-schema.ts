import { z, ZodError } from 'zod';

// ProseMirror document root used for rich text content in shapes
const ProseMirrorDocSchema = z.object({
  type: z.literal('doc'),
  content: z.array(z.any()), // keep flexible to validate actual PM nodes without tight coupling
});

// A text node for DGM.js diagrams, including a ProseMirror-compatible content blob
export const DgmjsTextSchema = z.object({
  type: z.literal('text'),
  // ProseMirror document for the text content
  content: ProseMirrorDocSchema,
});

// Base shape: Rectangle (expandable in the future)
const RectangleShapeSchema = z.object({
  id: z.string(),
  type: z.literal('Rectangle'),
  position: z.object({ x: z.number(), y: z.number() }),
  width: z.number(),
  height: z.number(),
  text: DgmjsTextSchema.optional(),
}).strict();

// Discriminated union for all shapes (currently only Rectangle is supported)
export const DgmjsShapeSchema = z.discriminatedUnion('type', [RectangleShapeSchema as const]);

export type DgmjsShape = z.infer<typeof DgmjsShapeSchema>;

// Connector between shapes
export const DgmjsConnectorSchema = z.object({
  id: z.string(),
  type: z.literal('Connector'),
  from: z.string(),
  to: z.string(),
  points: z.array(z.object({ x: z.number(), y: z.number() })).optional(),
}).strict();

export type DgmjsConnector = z.infer<typeof DgmjsConnectorSchema>;

// Shape position helper (for clarity / potential reuse)
export const ShapePositionSchema = z.object({ x: z.number(), y: z.number() });
export type ShapePosition = z.infer<typeof ShapePositionSchema>;

// Page root: contains shapes and optional connectors
export const DgmjsPageSchema = z.object({
  id: z.string(),
  name: z.string().optional(),
  shapes: z.array(DgmjsShapeSchema),
  connectors: z.array(DgmjsConnectorSchema).optional(),
}).strict();

// Document root
export const DgmjsDocumentSchema = z.object({ pages: z.array(DgmjsPageSchema) }).strict();
export type DgmjsDocument = z.infer<typeof DgmjsDocumentSchema>;
export type DgmjsPage = z.infer<typeof DgmjsPageSchema>;

// Validation wrapper
export function validateDgmjsJson(json: unknown): { valid: boolean; errors?: string[] } {
  try {
    DgmjsDocumentSchema.parse(json);
    return { valid: true };
  } catch (err) {
    if (err && err instanceof ZodError) {
      const messages = err.errors.map((e) => {
        const path = e.path.length ? e.path.join('.') : 'root';
        return `${path} - ${e.message}`;
      });
      return { valid: false, errors: messages };
    }
    return { valid: false, errors: ['Unknown validation error'] };
  }
}

export { ProseMirrorDocSchema as ProseMirrorDoc };
