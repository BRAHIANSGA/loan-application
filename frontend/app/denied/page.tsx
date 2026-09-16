import { CircleX } from "lucide-react";
import { ResultCard } from "@/components/result-card";

// Only reasons that are safe to share with an applicant get a specific message.
const SHAREABLE_REASONS: Record<string, string> = {
  STATE_NOT_ELIGIBLE: "We don't offer loans in your state yet.",
};

const GENERIC_REASON = "We couldn't approve your application at this time.";

export default async function DeniedPage({ searchParams }: PageProps<"/denied">) {
  const { reason } = await searchParams;
  const message = [reason ?? []].flat().map((code) => SHAREABLE_REASONS[code]).find(Boolean);

  return (
    <ResultCard
      icon={<CircleX className="size-12 text-destructive" aria-hidden />}
      title="Application not approved"
      description={message ?? GENERIC_REASON}
    />
  );
}
