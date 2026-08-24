export const STATUSES = [
  { id: 1, name: "To Do" },
  { id: 2, name: "In Progress" },
  { id: 3, name: "Completed" },
];

export const PRIORITIES = [
  { id: 1, name: "Low" },
  { id: 2, name: "Medium" },
  { id: 3, name: "High" },
];

export const CATEGORIES = [
  { id: 1, name: "Work" },
  { id: 2, name: "Personal" },
  { id: 3, name: "Urgent" },
  { id: 4, name: "Other" },
];

export function getNameById(list, id) {
  return list.find((item) => item.id === id)?.name ?? "Unknown";
}