// src/services/mockPayOS.ts

export interface PaymentTransaction {
  id: string;
  bookingId: string;
  amount: number;
  status: 'PENDING' | 'PAID' | 'EXPIRED' | 'CANCELLED';
  paymentUrl: string;
  payosQrData: string;
  createdAt: string;
  paidAt?: string;
}

const TRANSACTIONS_KEY = 'lockedin_transactions';

export function getMockTransactions(): PaymentTransaction[] {
  const data = localStorage.getItem(TRANSACTIONS_KEY);
  return data ? JSON.parse(data) : [];
}

export function saveMockTransactions(txs: PaymentTransaction[]): void {
  localStorage.setItem(TRANSACTIONS_KEY, JSON.stringify(txs));
}

export function createMockPaymentLink(bookingId: string, amount: number): PaymentTransaction {
  const id = 'payos-tx-' + Math.random().toString(36).substring(2, 11).toUpperCase();
  const paymentUrl = `/checkout/${id}`;
  const payosQrData = `payos-qr-mock-data-for-booking-${bookingId}-amount-${amount}`;

  const newTx: PaymentTransaction = {
    id,
    bookingId,
    amount,
    status: 'PENDING',
    paymentUrl,
    payosQrData,
    createdAt: new Date().toISOString()
  };

  const txs = getMockTransactions();
  txs.push(newTx);
  saveMockTransactions(txs);

  return newTx;
}

export function updateTransactionStatus(txId: string, status: 'PAID' | 'EXPIRED' | 'CANCELLED'): PaymentTransaction | null {
  const txs = getMockTransactions();
  const txIndex = txs.findIndex(t => t.id === txId);
  if (txIndex === -1) return null;

  txs[txIndex].status = status;
  if (status === 'PAID') {
    txs[txIndex].paidAt = new Date().toISOString();
  }

  saveMockTransactions(txs);
  return txs[txIndex];
}
