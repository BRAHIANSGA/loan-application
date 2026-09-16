import { z } from "zod";

const MIN_AMOUNT = 1;
const MAX_AMOUNT = 1_000_000_000;

const requiredText = (label: string, maxLength: number) =>
  z.string().trim().min(1, `${label} is required`).max(maxLength, `${label} is too long`);

// Same rule as the backend: the SSA never issues area 000, 666 or 900-999, group 00 or serial 0000.
const isIssuableSsn = (ssn: string) => {
  const [area = "", group = "", serial = ""] = ssn.split("-");
  return !["000", "666"].includes(area) && !area.startsWith("9") && group !== "00" && serial !== "0000";
};

export const loanApplicationSchema = z.object({
  firstName: requiredText("First name", 100),
  lastName: requiredText("Last name", 100),
  ssn: z
    .string()
    .trim()
    .regex(/^\d{3}-\d{2}-\d{4}$/, "Enter the SSN as 123-45-6789")
    .refine(isIssuableSsn, "Enter a valid SSN"),
  companyName: requiredText("Company name", 200),
  requestedAmount: z
    .string()
    .trim()
    .regex(/^\d+(\.\d{1,2})?$/, "Enter an amount such as 50000 or 50000.50")
    .refine((amount) => Number(amount) >= MIN_AMOUNT, "The minimum amount is $1")
    .refine((amount) => Number(amount) <= MAX_AMOUNT, "The maximum amount is $1,000,000,000"),
  address: z.object({
    street: requiredText("Street", 200),
    city: requiredText("City", 100),
    state: z.string({ error: "Select a state" }).length(2, "Select a state"),
    zipCode: z.string().trim().regex(/^\d{5}(-\d{4})?$/, "Enter a 5-digit ZIP code"),
  }),
});

export type LoanApplicationValues = z.infer<typeof loanApplicationSchema>;
