import { ActivatedRouteSnapshot, ResolveFn, RouterStateSnapshot } from '@angular/router';
import { Member } from '../_models/member';
import { MembersService } from '../_services/members.service';
import { inject } from '@angular/core';

export const memberDetailedResolver: ResolveFn<Member> = (route: ActivatedRouteSnapshot, state: RouterStateSnapshot) => {
      return inject(MembersService).getMember(route.paramMap.get('username')!);
    };