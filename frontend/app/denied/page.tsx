import { CircleX } from "lucide-react";
import { ResultCard } from "@/components/result-card";
import { SHAREABLE_DENIAL_REASONS } from "@/lib/denial-reasons";

const GENERIC_REASON = "We couldn't approve your application at this time.";

export default async function DeniedPage({ searchParams }: PageProps<"/denied">) {
  const { reason } = await searchParams;
  const message = [reason ?? []].flat().map((code) => SHAREABLE_DENIAL_REASONS[code]).find(Boolean);

  return (
    <ResultCard
      icon={<CircleX className="size-12 text-destructive" aria-hidden />}
      title="Application not approved"
      description={message ?? GENERIC_REASON}
    />
  );
}
