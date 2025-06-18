import {z} from 'zod';

export const RaritySchema = z.enum([
  "Common",
  "Uncommon",
  "Rare",
  "Epic",
  "Legendary",
]);

export type Rarity = z.infer<typeof RaritySchema>;
