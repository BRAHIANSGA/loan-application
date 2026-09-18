"use client";

import { useState, useTransition, type ComponentProps } from "react";
import { Controller, useForm, useWatch, type Control, type FieldPath } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { submitLoanApplication } from "@/app/actions";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import {
  Field,
  FieldDescription,
  FieldError,
  FieldGroup,
  FieldLabel,
  FieldLegend,
  FieldSet,
} from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { loanApplicationSchema, type LoanApplicationValues } from "@/lib/loan-application-schema";
import { US_STATES } from "@/lib/us-states";

const EMPTY_APPLICATION: LoanApplicationValues = {
  firstName: "",
  lastName: "",
  ssn: "",
  companyName: "",
  requestedAmount: "",
  address: { street: "", city: "", state: "", zipCode: "" },
};

const fieldErrorId = (name: string) => `${name}-error`;

export function LoanApplicationForm() {
  const [submitError, setSubmitError] = useState<string>();
  const [isSubmitting, startTransition] = useTransition();
  const { control, handleSubmit, setError } = useForm<LoanApplicationValues>({
    resolver: zodResolver(loanApplicationSchema),
    defaultValues: EMPTY_APPLICATION,
  });
  const requestedAmount = useWatch({ control, name: "requestedAmount" });

  const onSubmit = (values: LoanApplicationValues) =>
    startTransition(async () => {
      const result = await submitLoanApplication(values);
      setSubmitError(result?.error);
      for (const [name, message] of Object.entries(result?.fieldErrors ?? {})) {
        setError(name as FieldPath<LoanApplicationValues>, { message });
      }
    });

  return (
    <form onSubmit={handleSubmit(onSubmit)} noValidate>
      <FieldGroup>
        <FieldSet>
          <FieldLegend>About you</FieldLegend>
          <div className="grid gap-4 sm:grid-cols-2">
            <TextField control={control} name="firstName" label="First name" autoComplete="given-name" />
            <TextField control={control} name="lastName" label="Last name" autoComplete="family-name" />
          </div>
          <TextField
            control={control}
            name="ssn"
            label="Social Security number"
            description="Used only to evaluate this application."
            placeholder="123-45-6789"
            inputMode="numeric"
            autoComplete="off"
            format={formatSsn}
          />
        </FieldSet>

        <FieldSet>
          <FieldLegend>Your business</FieldLegend>
          <TextField control={control} name="companyName" label="Company name" autoComplete="organization" />
          <TextField
            control={control}
            name="requestedAmount"
            label="Requested amount (USD)"
            placeholder="50000"
            inputMode="decimal"
            format={formatAmount}
            description={describeAmount(requestedAmount)}
          />
        </FieldSet>

        <FieldSet>
          <FieldLegend>Address</FieldLegend>
          <TextField control={control} name="address.street" label="Street" autoComplete="street-address" />
          <div className="grid gap-4 sm:grid-cols-3">
            <TextField control={control} name="address.city" label="City" autoComplete="address-level2" />
            <Controller
              control={control}
              name="address.state"
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid}>
                  <FieldLabel htmlFor={field.name}>State</FieldLabel>
                  <Select
                    name={field.name}
                    items={US_STATES}
                    value={field.value || null}
                    onValueChange={field.onChange}
                  >
                    <SelectTrigger
                      id={field.name}
                      ref={field.ref}
                      aria-invalid={fieldState.invalid}
                      aria-describedby={fieldState.invalid ? fieldErrorId(field.name) : undefined}
                      className="w-full"
                    >
                      <SelectValue placeholder="Select" />
                    </SelectTrigger>
                    <SelectContent>
                      {US_STATES.map((state) => (
                        <SelectItem key={state.value} value={state.value}>
                          {state.label}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <FieldError id={fieldErrorId(field.name)} errors={[fieldState.error]} />
                </Field>
              )}
            />
            <TextField
              control={control}
              name="address.zipCode"
              label="ZIP code"
              inputMode="numeric"
              autoComplete="postal-code"
            />
          </div>
        </FieldSet>

        {submitError && (
          <Alert variant="destructive">
            <AlertDescription>{submitError}</AlertDescription>
          </Alert>
        )}

        <Button type="submit" size="lg" disabled={isSubmitting}>
          {isSubmitting ? "Submitting..." : "Submit application"}
        </Button>
      </FieldGroup>
    </form>
  );
}

type TextFieldProps = Omit<ComponentProps<typeof Input>, "name"> & {
  control: Control<LoanApplicationValues>;
  name: Exclude<FieldPath<LoanApplicationValues>, "address">;
  label: string;
  description?: string;
  format?: (value: string) => string;
};

function TextField({ control, name, label, description, format, ...inputProps }: TextFieldProps) {
  return (
    <Controller
      control={control}
      name={name}
      render={({ field, fieldState }) => (
        <Field data-invalid={fieldState.invalid}>
          <FieldLabel htmlFor={field.name}>{label}</FieldLabel>
          <Input
            {...inputProps}
            {...field}
            id={field.name}
            aria-invalid={fieldState.invalid}
            aria-describedby={fieldState.invalid ? fieldErrorId(field.name) : undefined}
            onChange={(event) => field.onChange(format ? format(event.target.value) : event.target.value)}
          />
          {description && <FieldDescription>{description}</FieldDescription>}
          <FieldError id={fieldErrorId(field.name)} errors={[fieldState.error]} />
        </Field>
      )}
    />
  );
}

function formatSsn(value: string) {
  const digits = value.replace(/\D/g, "").slice(0, 9);
  return [digits.slice(0, 3), digits.slice(3, 5), digits.slice(5)].filter(Boolean).join("-");
}

const USD = new Intl.NumberFormat("en-US", { style: "currency", currency: "USD" });

function formatAmount(value: string) {
  const [whole, ...decimals] = value.replace(/[^\d.]/g, "").split(".");
  return decimals.length > 0 ? `${whole}.${decimals.join("").slice(0, 2)}` : whole;
}

function describeAmount(amount: string) {
  return Number(amount) > 0 ? `You are requesting ${USD.format(Number(amount))}` : undefined;
}
