using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using NobatPlusDATA.ViewModels;
using static NobatPlusDATA.Tools.DbTools;

namespace NobatPlusDATA.DataLayer.Services
{
    public class SupportTicketRep : ISupportTicketRep
    {
        private readonly NobatPlusContext _context;

        public SupportTicketRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<ListResultObject<SupportTicketVM>> GetAllSupportTicketsAsync(
            long personId = 0,
            string status = "",
            string priority = "",
            string category = "",
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int pageIndex = 1,
            int pageSize = 20,
            string searchText = "",
            string sortQuery = "")
        {
            var result = new ListResultObject<SupportTicketVM>();
            try
            {
                var query = _context.SupportTickets
                    .AsNoTracking()
                    .Include(x => x.Person)
                    .Include(x => x.AssignedAdminPerson)
                    .Include(x => x.Messages)
                    .AsQueryable();

                if (personId > 0) query = query.Where(x => x.PersonID == personId);
                if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status == status.Trim());
                if (!string.IsNullOrWhiteSpace(priority)) query = query.Where(x => x.Priority == priority.Trim());
                if (!string.IsNullOrWhiteSpace(category)) query = query.Where(x => x.Category == category.Trim());

                if (fromDate.HasValue)
                {
                    var from = fromDate.Value.ToShamsi();
                    query = query.Where(x => x.CreateDate >= from);
                }

                if (toDate.HasValue)
                {
                    var to = toDate.Value.ToShamsi();
                    query = query.Where(x => x.CreateDate <= to);
                }

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    var text = searchText.Trim();
                    query = query.Where(x =>
                        x.ID.ToString().Contains(text) ||
                        x.Title.Contains(text) ||
                        x.Category.Contains(text) ||
                        x.Priority.Contains(text) ||
                        x.Status.Contains(text) ||
                        (x.Description != null && x.Description.Contains(text)) ||
                        x.Person.FirstName.Contains(text) ||
                        x.Person.LastName.Contains(text) ||
                        x.Person.PhoneNumber.Contains(text) ||
                        (x.Person.Email != null && x.Person.Email.Contains(text)) ||
                        x.Messages.Any(m => m.Message.Contains(text)));
                }

                result.TotalCount = await query.CountAsync();
                result.PageCount = GetPageCount(result.TotalCount, pageSize);

                result.Results = await query
                    .OrderByDescending(x => x.LastMessageAt)
                    .SortBy(sortQuery)
                    .ToPaging(pageIndex, pageSize)
                    .Select(x => new SupportTicketVM
                    {
                        ID = x.ID,
                        PersonID = x.PersonID,
                        PersonFullName = (x.Person.FirstName + " " + x.Person.LastName).Trim(),
                        PersonPhoneNumber = x.Person.PhoneNumber,
                        PersonEmail = x.Person.Email ?? "",
                        Title = x.Title,
                        Category = x.Category,
                        Priority = x.Priority,
                        Status = x.Status,
                        AssignedAdminPersonID = x.AssignedAdminPersonID,
                        AssignedAdminName = x.AssignedAdminPerson == null ? "" : (x.AssignedAdminPerson.FirstName + " " + x.AssignedAdminPerson.LastName).Trim(),
                        CreateDate = x.CreateDate,
                        UpdateDate = x.UpdateDate,
                        LastMessageAt = x.LastMessageAt,
                        ClosedAt = x.ClosedAt,
                        Description = x.Description ?? "",
                        MessageCount = x.Messages.Count,
                        LastMessage = x.Messages
                            .OrderByDescending(m => m.CreateDate)
                            .Select(m => m.Message)
                            .FirstOrDefault() ?? ""
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }

            return result;
        }

        public async Task<RowResultObject<SupportTicketVM>> GetSupportTicketByIdAsync(long id)
        {
            var result = new RowResultObject<SupportTicketVM>();
            try
            {
                result.Result = await _context.SupportTickets
                    .AsNoTracking()
                    .Include(x => x.Person)
                    .Include(x => x.AssignedAdminPerson)
                    .Include(x => x.Messages).ThenInclude(x => x.SenderPerson)
                    .Where(x => x.ID == id)
                    .Select(x => new SupportTicketVM
                    {
                        ID = x.ID,
                        PersonID = x.PersonID,
                        PersonFullName = (x.Person.FirstName + " " + x.Person.LastName).Trim(),
                        PersonPhoneNumber = x.Person.PhoneNumber,
                        PersonEmail = x.Person.Email ?? "",
                        Title = x.Title,
                        Category = x.Category,
                        Priority = x.Priority,
                        Status = x.Status,
                        AssignedAdminPersonID = x.AssignedAdminPersonID,
                        AssignedAdminName = x.AssignedAdminPerson == null ? "" : (x.AssignedAdminPerson.FirstName + " " + x.AssignedAdminPerson.LastName).Trim(),
                        CreateDate = x.CreateDate,
                        UpdateDate = x.UpdateDate,
                        LastMessageAt = x.LastMessageAt,
                        ClosedAt = x.ClosedAt,
                        Description = x.Description ?? "",
                        MessageCount = x.Messages.Count,
                        LastMessage = x.Messages.OrderByDescending(m => m.CreateDate).Select(m => m.Message).FirstOrDefault() ?? "",
                        Messages = x.Messages
                            .OrderBy(m => m.CreateDate)
                            .Select(m => new SupportTicketMessageVM
                            {
                                ID = m.ID,
                                SupportTicketID = m.SupportTicketID,
                                SenderPersonID = m.SenderPersonID,
                                SenderFullName = (m.SenderPerson.FirstName + " " + m.SenderPerson.LastName).Trim(),
                                SenderPhoneNumber = m.SenderPerson.PhoneNumber,
                                IsAdminReply = m.IsAdminReply,
                                Message = m.Message,
                                CreateDate = m.CreateDate,
                                UpdateDate = m.UpdateDate,
                                Description = m.Description ?? ""
                            }).ToList()
                    })
                    .SingleOrDefaultAsync();

                if (result.Result == null)
                {
                    result.Status = false;
                    result.ErrorMessage = "تیکت یافت نشد";
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }

            return result;
        }

        public async Task<BitResultObject> AddSupportTicketAsync(SupportTicket ticket, string firstMessage)
        {
            var result = new BitResultObject();
            try
            {
                var now = DateTime.Now.ToShamsi();
                ticket.CreateDate = now;
                ticket.UpdateDate = now;
                ticket.LastMessageAt = now;
                ticket.Status = NormalizeStatus(ticket.Status, "open");
                ticket.Priority = NormalizePriority(ticket.Priority);
                ticket.Category = ticket.Category?.Trim() ?? "";
                ticket.Messages = new List<SupportTicketMessage>
                {
                    new()
                    {
                        SenderPersonID = ticket.PersonID,
                        IsAdminReply = false,
                        Message = firstMessage,
                        CreateDate = now,
                        UpdateDate = now
                    }
                };

                await _context.SupportTickets.AddAsync(ticket);
                await _context.SaveChangesAsync();
                result.ID = ticket.ID;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }

            return result;
        }

        public async Task<BitResultObject> AddSupportTicketMessageAsync(long ticketId, long senderPersonId, bool isAdminReply, string message, string nextStatus = "")
        {
            var result = new BitResultObject();
            try
            {
                var ticket = await _context.SupportTickets.SingleOrDefaultAsync(x => x.ID == ticketId);
                if (ticket == null)
                {
                    result.Status = false;
                    result.ErrorMessage = "تیکت یافت نشد";
                    return result;
                }

                var now = DateTime.Now.ToShamsi();
                var row = new SupportTicketMessage
                {
                    SupportTicketID = ticketId,
                    SenderPersonID = senderPersonId,
                    IsAdminReply = isAdminReply,
                    Message = message,
                    CreateDate = now,
                    UpdateDate = now
                };

                ticket.LastMessageAt = now;
                ticket.UpdateDate = now;
                if (isAdminReply && ticket.AssignedAdminPersonID == null)
                    ticket.AssignedAdminPersonID = senderPersonId;

                if (!string.IsNullOrWhiteSpace(nextStatus))
                    ticket.Status = NormalizeStatus(nextStatus, ticket.Status);
                else if (isAdminReply && ticket.Status == "open")
                    ticket.Status = "in_progress";

                ticket.ClosedAt = ticket.Status == "closed" ? now : null;

                await _context.SupportTicketMessages.AddAsync(row);
                await _context.SaveChangesAsync();
                result.ID = row.ID;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }

            return result;
        }

        public async Task<BitResultObject> UpdateSupportTicketStatusAsync(long ticketId, string status, long? assignedAdminPersonId = null)
        {
            var result = new BitResultObject();
            try
            {
                var ticket = await _context.SupportTickets.SingleOrDefaultAsync(x => x.ID == ticketId);
                if (ticket == null)
                {
                    result.Status = false;
                    result.ErrorMessage = "تیکت یافت نشد";
                    return result;
                }

                var now = DateTime.Now.ToShamsi();
                ticket.Status = NormalizeStatus(status, ticket.Status);
                ticket.AssignedAdminPersonID = assignedAdminPersonId ?? ticket.AssignedAdminPersonID;
                ticket.ClosedAt = ticket.Status == "closed" ? now : null;
                ticket.UpdateDate = now;

                await _context.SaveChangesAsync();
                result.ID = ticket.ID;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }

            return result;
        }

        public async Task<BitResultObject> RemoveSupportTicketAsync(long id)
        {
            var result = new BitResultObject();
            try
            {
                var ticket = await _context.SupportTickets.SingleOrDefaultAsync(x => x.ID == id);
                if (ticket == null)
                {
                    result.Status = false;
                    result.ErrorMessage = "تیکت یافت نشد";
                    return result;
                }

                _context.SupportTickets.Remove(ticket);
                await _context.SaveChangesAsync();
                result.ID = id;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }

            return result;
        }

        private static string NormalizeStatus(string? status, string fallback)
        {
            var value = (status ?? "").Trim().ToLower();
            return value is "open" or "in_progress" or "waiting_user" or "closed" ? value : fallback;
        }

        private static string NormalizePriority(string? priority)
        {
            var value = (priority ?? "").Trim().ToLower();
            return value is "low" or "normal" or "high" or "urgent" ? value : "normal";
        }
    }
}
