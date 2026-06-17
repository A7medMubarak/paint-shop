import { useState, useEffect } from 'react';
import { api } from '../services/api';
import type { UserDto } from '../types';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { ErrorMessage } from '../components/ErrorMessage';
import { EmptyState } from '../components/EmptyState';
import { ConfirmDialog } from '../components/ConfirmDialog';
import toast from 'react-hot-toast';

export default function UsersPage() {
  const [users, setUsers] = useState<UserDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showCreate, setShowCreate] = useState(false);
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [deactivateId, setDeactivateId] = useState<number | null>(null);
  const [editId, setEditId] = useState<number | null>(null);
  const [editUsername, setEditUsername] = useState('');
  const [editRole, setEditRole] = useState('');
  const [resetPwdId, setResetPwdId] = useState<number | null>(null);
  const [newPassword, setNewPassword] = useState('');

  useEffect(() => {
    let ignore = false;
    (async () => {
      try {
        const data = await api.get<UserDto[]>('/users');
        if (!ignore) setUsers(data);
      } catch (err) {
        if (!ignore) setError(err instanceof Error ? err.message : 'Failed to load users');
      } finally { if (!ignore) setLoading(false); }
    })();
    return () => { ignore = true; };
  }, []);

  const handleCreate = async () => {
    if (!username.trim() || !password.trim()) { toast.error('All fields required'); return; }
    try {
      await api.post('/users', { username, password });
      toast.success('Employee created');
      setShowCreate(false); setUsername(''); setPassword('');
      setLoading(true); const data = await api.get<UserDto[]>('/users'); setUsers(data); setLoading(false);
    } catch { toast.error('Failed'); setLoading(false); }
  };

  const handleDeactivate = async () => {
    if (!deactivateId) return;
    try {
      await api.patch(`/users/${deactivateId}/deactivate`);
      toast.success('User deactivated');
      setDeactivateId(null);
      setLoading(true); const data = await api.get<UserDto[]>('/users'); setUsers(data); setLoading(false);
    } catch { toast.error('Failed'); setLoading(false); }
  };

  const handleEdit = async () => {
    if (!editUsername.trim()) { toast.error('Username required'); return; }
    try {
      await api.put(`/users/${editId}`, { username: editUsername, role: editRole });
      toast.success('User updated');
      setEditId(null);
      setLoading(true); const data = await api.get<UserDto[]>('/users'); setUsers(data); setLoading(false);
    } catch { toast.error('Failed'); setLoading(false); }
  };

  const handleResetPassword = async () => {
    if (!newPassword.trim()) { toast.error('Password required'); return; }
    try {
      await api.patch(`/users/${resetPwdId}/password`, { newPassword });
      toast.success('Password reset');
      setResetPwdId(null); setNewPassword('');
    } catch { toast.error('Failed'); }
  };

  if (loading) return <LoadingSpinner />;
  if (error) return <ErrorMessage message={error} onRetry={() => window.location.reload()} />;

  return (
    <div>
      <div className="mb-4 flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">Users</h1>
        <button onClick={() => setShowCreate(!showCreate)} className="rounded-md bg-blue-600 px-4 py-2 text-sm text-white hover:bg-blue-700">
          {showCreate ? 'Cancel' : 'New Employee'}
        </button>
      </div>

      {showCreate && (
        <div className="mb-6 rounded-lg border bg-white p-4 shadow-sm">
          <h3 className="mb-3 font-medium">New Employee</h3>
          <div className="grid gap-3 sm:grid-cols-3">
            <input type="text" placeholder="Username" value={username} onChange={e => setUsername(e.target.value)} className="rounded-md border px-3 py-2 text-sm" />
            <input type="password" placeholder="Password" value={password} onChange={e => setPassword(e.target.value)} className="rounded-md border px-3 py-2 text-sm" />
            <button onClick={handleCreate} className="rounded-md bg-green-600 px-3 py-2 text-sm text-white hover:bg-green-700">Save</button>
          </div>
        </div>
      )}

      {users.length === 0 ? <EmptyState message="No users" /> : (
        <div className="overflow-x-auto rounded-lg border bg-white shadow-sm">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 text-left">
              <tr>
                <th className="px-4 py-3 font-medium text-gray-600">Username</th>
                <th className="px-4 py-3 font-medium text-gray-600">Role</th>
                <th className="px-4 py-3 font-medium text-gray-600">Created</th>
                <th className="px-4 py-3" />
              </tr>
            </thead>
            <tbody>
              {users.map((u) => (
                <tr key={u.id} className="border-t">
                  <td className="px-4 py-3 font-medium">{u.username}</td>
                  <td className="px-4 py-3">
                    <span className={`rounded-full px-2 py-0.5 text-xs font-medium ${u.role === 'Owner' ? 'bg-purple-100 text-purple-700' : 'bg-blue-100 text-blue-700'}`}>{u.role}</span>
                  </td>
                  <td className="px-4 py-3 text-gray-500">{new Date(u.createdAt).toLocaleDateString()}</td>
                  <td className="px-4 py-3">
                    <div className="flex gap-2">
                      {u.role !== 'Owner' && (
                        <>
                          <button onClick={() => { setEditId(u.id); setEditUsername(u.username); setEditRole(u.role); }} className="text-xs text-blue-600 hover:text-blue-800">Edit</button>
                          <button onClick={() => { setResetPwdId(u.id); setNewPassword(''); }} className="text-xs text-green-600 hover:text-green-800">Password</button>
                          <button onClick={() => setDeactivateId(u.id)} className="text-xs text-red-600 hover:text-red-800">Deactivate</button>
                        </>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      <ConfirmDialog open={deactivateId !== null} title="Deactivate User" message="This will prevent the user from logging in. Continue?" confirmLabel="Deactivate" onConfirm={handleDeactivate} onCancel={() => setDeactivateId(null)} />

      {editId !== null && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/30" onClick={() => setEditId(null)}>
          <div className="w-full max-w-md rounded-lg bg-white p-6 shadow-xl" onClick={e => e.stopPropagation()}>
            <h3 className="mb-4 text-lg font-medium">Edit User</h3>
            <div className="space-y-3">
              <input type="text" value={editUsername} onChange={e => setEditUsername(e.target.value)} className="w-full rounded-md border px-3 py-2 text-sm" placeholder="Username" />
              <select value={editRole} onChange={e => setEditRole(e.target.value)} className="w-full rounded-md border px-3 py-2 text-sm">
                <option value="Employee">Employee</option>
                <option value="Owner">Owner</option>
              </select>
            </div>
            <div className="mt-4 flex justify-end gap-2">
              <button onClick={() => setEditId(null)} className="rounded-md border px-3 py-1.5 text-sm text-gray-700">Cancel</button>
              <button onClick={handleEdit} className="rounded-md bg-blue-600 px-3 py-1.5 text-sm text-white hover:bg-blue-700">Save</button>
            </div>
          </div>
        </div>
      )}

      {resetPwdId !== null && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/30" onClick={() => setResetPwdId(null)}>
          <div className="w-full max-w-md rounded-lg bg-white p-6 shadow-xl" onClick={e => e.stopPropagation()}>
            <h3 className="mb-4 text-lg font-medium">Reset Password</h3>
            <input type="password" value={newPassword} onChange={e => setNewPassword(e.target.value)} className="w-full rounded-md border px-3 py-2 text-sm" placeholder="New password" />
            <div className="mt-4 flex justify-end gap-2">
              <button onClick={() => setResetPwdId(null)} className="rounded-md border px-3 py-1.5 text-sm text-gray-700">Cancel</button>
              <button onClick={handleResetPassword} className="rounded-md bg-blue-600 px-3 py-1.5 text-sm text-white hover:bg-blue-700">Reset</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
