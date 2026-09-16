import { z } from "zod";

const requiredText = (label: string, maxLength: number) =>
  z.string().trim().min(1, `${label} is required`).max(maxLength, `${label} is too long`);

export const loanApplicationSchema = z.object({
  firstName: requiredText("First name", 100),
  lastName: requiredText("Last name", 100),
  ssn: z.string().regex(/^\d{3}-\d{2}-\d{4}$/, "Enter the SSN as 123-45-6789"),
  companyName: requiredText("Company name", 200),
  requestedAmount: z
    .string()
    .regex(/^\d+(\.\d{1,2})?$/, "Enter an amount such as 50000")
    .refine((amount) => Number(amount) > 0, "The amount must be greater than zero"),
  address: z.object({
    street: requiredText("Street", 200),
    city: requiredText("City", 100),
    state: z.string({ error: "Select a state" }).length(2, "Select a state"),
    zipCode: z.string().regex(/^\d{5}(-\d{4})?$/, "Enter a 5-digit ZIP code"),
  }),
});

export type LoanApplicationValues = z.infer<typeof loanApplicationSchema>;
