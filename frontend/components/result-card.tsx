import Link from "next/link";
import type { ReactNode } from "react";
import { buttonVariants } from "@/components/ui/button";
import { Card, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";

type ResultCardProps = {
  icon: ReactNode;
  title: string;
  description: string;
  children?: ReactNode;
};

export function ResultCard({ icon, title, description, children }: ResultCardProps) {
  return (
    <main className="mx-auto w-full max-w-lg px-4 py-16">
      <Card>
        <CardHeader className="justify-items-center gap-3 text-center">
          {icon}
          <CardTitle className="text-xl">{title}</CardTitle>
          <CardDescription>{description}</CardDescription>
        </CardHeader>
        {children}
        <CardFooter className="justify-center">
          <Link href="/" className={buttonVariants({ variant: "outline" })}>
            Back to the application
          </Link>
        </CardFooter>
      </Card>
    </main>
  );
}
