"use server";

import { redirect } from "next/navigation";
import { loanApplicationSchema, type LoanApplicationValues } from "@/lib/loan-application-schema";

type SubmitLoanApplicationResponse = {
  decision: "Approved" | "Denied";
  denialReasons: string[];
  loanApplicationId: string | null;
  isReturningCustomer: boolean;
};

const API_URL = process.env.API_URL ?? "http://localhost:5080";
const SUBMIT_ERROR = "We could not submit your application. Please try again in a moment.";

export async function submitLoanApplication(values: LoanApplicationValues) {
  const parsed = loanApplicationSchema.safeParse(values);
  if (!parsed.success) {
    return { error: "Please review the highlighted fields." };
  }

  let result: SubmitLoanApplicationResponse;
  try {
    const response = await fetch(`${API_URL}/api/loan-applications`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ ...parsed.data, requestedAmount: Number(parsed.data.requestedAmount) }),
    });

    if (!response.ok) {
      return { error: SUBMIT_ERROR };
    }

    result = await response.json();
  } catch {
    return { error: SUBMIT_ERROR };
  }

  if (result.decision === "Denied") {
    const reasons = new URLSearchParams(result.denialReasons.map((reason) => ["reason", reason]));
    redirect(`/denied?${reasons}`);
  }

  const outcome = new URLSearchParams({
    applicationId: result.loanApplicationId ?? "",
    returning: String(result.isReturningCustomer),
  });
  redirect(`/approved?${outcome}`);
}
