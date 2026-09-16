import { LoanApplicationForm } from "@/components/loan-application-form";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";

export default function HomePage() {
  return (
    <main className="mx-auto w-full max-w-2xl px-4 py-10">
      <Card>
        <CardHeader>
          <CardTitle className="text-xl">Small business loan application</CardTitle>
          <CardDescription>Tell us about you and your company. You will get an answer right away.</CardDescription>
        </CardHeader>
        <CardContent>
          <LoanApplicationForm />
        </CardContent>
      </Card>
    </main>
  );
}
