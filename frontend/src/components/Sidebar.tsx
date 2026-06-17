import { useState } from 'react';
import { NavLink, useLocation } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';

const navItems = [
  { label: 'Dashboard', path: '/', roles: ['Owner', 'Employee'] },
  { label: 'Products', path: '/products', roles: ['Owner', 'Employee'] },
  { label: 'Inventory', path: '/inventory', roles: ['Owner', 'Employee'] },
  { label: 'Sales', path: '/sales', roles: ['Owner', 'Employee'] },
  { label: 'Customers', path: '/customers', roles: ['Owner', 'Employee'] },
  { label: 'Users', path: '/users', roles: ['Owner'] },
];

const reportSubItems = [
  { label: 'Daily Report', path: '/reports/daily' },
  { label: 'Period Report', path: '/reports/period' },
  { label: 'Top Selling', path: '/reports/top-selling' },
  { label: 'Low Stock', path: '/reports/low-stock' },
  { label: 'Inventory Value', path: '/reports/inventory-valuation' },
];

export function Sidebar({ open, onClose }: { open: boolean; onClose: () => void }) {
  const { isOwner } = useAuth();
  const location = useLocation();
  const isOnReport = location.pathname.startsWith('/reports');
  const [reportsOpen, setReportsOpen] = useState(isOnReport);

  const visible = navItems.filter(i => i.roles.includes(isOwner ? 'Owner' : 'Employee'));

  return (
    <>
      {open && <div className="fixed inset-0 z-30 bg-black/30 lg:hidden" onClick={onClose} />}
      <aside className={`fixed inset-y-0 left-0 z-40 w-64 transform bg-white shadow-lg transition-transform duration-200 lg:static lg:translate-x-0 ${open ? 'translate-x-0' : '-translate-x-full'}`}>
        <div className="flex h-16 items-center border-b px-6">
          <h2 className="text-lg font-bold text-gray-900">PaintShop</h2>
        </div>
        <nav className="mt-4 space-y-1 px-3">
          {visible.map((item) => (
            <NavLink
              key={item.path}
              to={item.path}
              end
              onClick={onClose}
              className={({ isActive }) =>
                `block rounded-md px-3 py-2 text-sm font-medium transition-colors ${
                  isActive ? 'bg-blue-50 text-blue-700' : 'text-gray-700 hover:bg-gray-100'
                }`
              }
            >
              {item.label}
            </NavLink>
          ))}
          {isOwner && (
            <div>
              <button
                onClick={() => setReportsOpen(!reportsOpen)}
                className="flex w-full items-center justify-between rounded-md px-3 py-2 text-sm font-medium text-gray-700 hover:bg-gray-100 transition-colors"
              >
                <span>Reports</span>
                <svg className={`h-4 w-4 transition-transform ${reportsOpen ? 'rotate-90' : ''}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                </svg>
              </button>
              {reportsOpen && (
                <div className="ml-3 mt-1 space-y-1 border-l-2 border-blue-200 pl-2">
                  {reportSubItems.map((item) => (
                    <NavLink
                      key={item.path}
                      to={item.path}
                      end
                      onClick={onClose}
                      className={({ isActive }) =>
                        `block rounded-md px-3 py-1.5 text-sm transition-colors ${
                          isActive ? 'bg-blue-50 text-blue-700 font-medium' : 'text-gray-600 hover:bg-gray-100'
                        }`
                      }
                    >
                      {item.label}
                    </NavLink>
                  ))}
                </div>
              )}
            </div>
          )}
        </nav>
      </aside>
    </>
  );
}
