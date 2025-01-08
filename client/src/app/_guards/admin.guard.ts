import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { AccountsService } from '../_services/accounts.service';
import { ToastrService } from 'ngx-toastr';

export const adminGuard: CanActivateFn = (route, state) => {
  const accountService = inject(AccountsService);
  const toastr = inject(ToastrService);

  if (accountService.roles().includes("Admin") || accountService.roles().includes("Moderator")) {
    return true;
  } else {
    toastr.error("Unauthorized access");
    return false;
  }
};
