import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "Loan Application",
  description: "Apply for a small business loan",
};

export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    <html lang="en" className="h-full antialiased">
      <body className="min-h-full flex flex-col bg-muted">{children}</body>
    </html>
  );
}
