import { CircleCheck } from "lucide-react";
import { ResultCard } from "@/components/result-card";
import { CardContent } from "@/components/ui/card";

export default async function ApprovedPage({ searchParams }: PageProps<"/approved">) {
  const { applicationId, returning } = await searchParams;
  const isReturningCustomer = returning === "true";

  return (
    <ResultCard
      icon={<CircleCheck className="size-12 text-emerald-600" aria-hidden />}
      title={isReturningCustomer ? "Application updated" : "Application approved"}
      description={
        isReturningCustomer
          ? "We found your previous application and updated it with these details."
          : "Your application passed our initial review. Our team will contact you soon."
      }
    >
      {applicationId && (
        <CardContent className="text-center text-sm text-muted-foreground">
          Reference number <span className="block font-mono text-foreground">{applicationId}</span>
        </CardContent>
      )}
    </ResultCard>
  );
}
