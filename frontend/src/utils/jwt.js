export function decodeToken(token) {
  try {
    const payload = token.split(".")[1];
    const decoded = JSON.parse(atob(payload.replace(/-/g, "+").replace(/_/g, "/")));
    return decoded;
  } catch {
    return null;
  }
}

export function getUserIdFromToken(token) {
  const decoded = decodeToken(token);
  return decoded?.sub ? Number(decoded.sub) : null;
}