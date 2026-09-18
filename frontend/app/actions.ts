"use server";

import { redirect } from "next/navigation";
import { SHAREABLE_DENIAL_REASONS } from "@/lib/denial-reasons";
import { loanApplicationSchema, type LoanApplicationValues } from "@/lib/loan-application-schema";

type SubmitLoanApplicationResponse = {
  decision: "Approved" | "Denied";
  denialReasons: string[];
  loanApplicationId: string | null;
  isReturningCustomer: boolean;
};

type SubmissionFailure = { error: string; fieldErrors?: Record<string, string> };

const API_URL = process.env.API_URL ?? "http://localhost:5080";
const INVALID_INPUT_ERROR = "Please review the highlighted fields.";
const UNAVAILABLE_ERROR = "We could not submit your application. Please try again in a moment.";

export async function submitLoanApplication(values: LoanApplicationValues): Promise<SubmissionFailure> {
  const parsed = loanApplicationSchema.safeParse(values);
  if (!parsed.success) {
    return { error: INVALID_INPUT_ERROR };
  }

  let result: SubmitLoanApplicationResponse;
  try {
    const response = await fetch(`${API_URL}/api/loan-applications`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ ...parsed.data, requestedAmount: Number(parsed.data.requestedAmount) }),
    });

    if (response.status === 400) {
      return { error: INVALID_INPUT_ERROR, fieldErrors: await readFieldErrors(response) };
    }
    if (!response.ok) {
      return { error: UNAVAILABLE_ERROR };
    }

    result = await response.json();
  } catch {
    return { error: UNAVAILABLE_ERROR };
  }

  if (result.decision === "Denied") {
    // Query strings end up in the address bar and the history: shareable codes only.
    const shareable = result.denialReasons.filter((reason) => reason in SHAREABLE_DENIAL_REASONS);
    const query = new URLSearchParams(shareable.map((reason) => ["reason", reason])).toString();
    redirect(query ? `/denied?${query}` : "/denied");
  }

  const outcome = new URLSearchParams({
    applicationId: result.loanApplicationId ?? "",
    returning: String(result.isReturningCustomer),
  });
  redirect(`/approved?${outcome}`);
}

// The API reports "Address.ZipCode"; the form names the same field "address.zipCode".
async function readFieldErrors(response: Response) {
  const problem: { errors?: Record<string, string[]> } = await response.json();

  return Object.fromEntries(
    Object.entries(problem.errors ?? {}).map(([field, messages]) => [
      field.split(".").map((part) => part.charAt(0).toLowerCase() + part.slice(1)).join("."),
      messages[0],
    ]),
  );
}
