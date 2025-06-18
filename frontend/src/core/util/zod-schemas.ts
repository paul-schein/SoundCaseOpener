import {z} from 'zod';
import {Instant} from '@js-joda/core';

export const InstantSchema = z.string().refine(
  (value) => {
    try {
      Instant.parse(value);
      return true;
    } catch {
      return false;
    }
  }, {
    message: 'The value is not a valid Instant.'
  }).transform((value) => Instant.parse(value));

export const RaritySchema = z.enum([
  "Common",
  "Uncommon",
  "Rare",
  "Epic",
  "Legendary",
]);

export type Rarity = z.infer<typeof RaritySchema>;

export const RoleSchema = z.enum(['User', 'Admin']);
